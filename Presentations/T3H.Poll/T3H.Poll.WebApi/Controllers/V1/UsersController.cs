
using T3H.Poll.Application.Common.Commands;
using T3H.Poll.Application.Users.Commands;
using T3H.Poll.Application.Users.Queries;
using T3H.Poll.CrossCuttingConcerns.ExtensionMethods;
using T3H.Poll.WebApi.Models.Users;

namespace T3H.Poll.WebApi.Controllers.V1;

[EnableRateLimiting(RateLimiterPolicyNames.DefaultPolicy)]
// [Authorize]
[Produces("application/json")]
[ApiController]
[ApiVersion("1.0")]
[Route("/api/v{version:apiVersion}/[controller]/")]
public class UsersController : ControllerBase
{
    private readonly Dispatcher _dispatcher;
    private readonly UserManager<User> _userManager;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly AppSettings _appSettings;

    public UsersController(Dispatcher dispatcher,
        UserManager<User> userManager,
        ILogger<UsersController> logger,
        IDateTimeProvider dateTimeProvider,
        IOptionsSnapshot<AppSettings> appSettings)
    {
        _dispatcher = dispatcher;
        _userManager = userManager;
        _dateTimeProvider = dateTimeProvider;
        _appSettings = appSettings.Value;
    }

   // [Authorize(AuthorizationPolicyNames.GetUsersPolicy)]
    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<IEnumerable<User>>> Get()
    {
        var users = await _dispatcher.DispatchAsync(new GetUsersQuery());
        var model = users.ToModels();
        return Ok(model);
    }

  //  [Authorize(AuthorizationPolicyNames.GetUserPolicy)]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<User>> Get(Guid id)
    {
        var user = await _dispatcher.DispatchAsync(new GetUserQuery { Id = id, AsNoTracking = true });
        var model = user.ToModel();
        return Ok(model);
    }

   // [Authorize(AuthorizationPolicyNames.GetUserPolicy)]
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<User>> Search([FromQuery]string name)
    {
        var user = await _dispatcher.DispatchAsync(new GetUsersByNameQuery { UserName = name, AsNoTracking = true });        
        return Ok(user);
    }


    //[Authorize(AuthorizationPolicyNames.AddUserPolicy)]
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<User>> Post([FromBody] UserModel model)
    {
        User user = new User
        {
            UserName = model.UserName,
            NormalizedUserName = model.UserName.ToUpperInvariantCulture(),
            Email = model.Email,
            NormalizedEmail = model.Email.ToUpperInvariantCulture(),
            EmailConfirmed = model.EmailConfirmed,
            PhoneNumber = model.PhoneNumber,
            PhoneNumberConfirmed = model.PhoneNumberConfirmed,
            TwoFactorEnabled = model.TwoFactorEnabled,
            LockoutEnabled = model.LockoutEnabled,
            LockoutEnd = model.LockoutEnd,
            AccessFailedCount = model.AccessFailedCount,
        };

        _ = await _userManager.CreateAsync(user);

        model = user.ToModel();
        return Created($"/api/users/{model.Id}", model);
    }

   // [Authorize(AuthorizationPolicyNames.UpdateUserPolicy)]
    [HttpPut("{id}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> Put(Guid id, [FromBody] UserModel model)
    {
        User user = await _dispatcher.DispatchAsync(new GetUserQuery { Id = id });

        user.UserName = model.UserName;
        user.NormalizedUserName = model.UserName.ToUpperInvariantCulture();
        user.Email = model.Email;
        user.NormalizedEmail = model.Email.ToUpperInvariantCulture();
        user.EmailConfirmed = model.EmailConfirmed;
        user.PhoneNumber = model.PhoneNumber;
        user.PhoneNumberConfirmed = model.PhoneNumberConfirmed;
        user.TwoFactorEnabled = model.TwoFactorEnabled;
        user.LockoutEnabled = model.LockoutEnabled;
        user.LockoutEnd = model.LockoutEnd;
        user.AccessFailedCount = model.AccessFailedCount;

        _ = await _userManager.UpdateAsync(user);

        model = user.ToModel();
        return Ok(model);
    }

    //[Authorize(AuthorizationPolicyNames.SetPasswordPolicy)]
    [HttpPut("{id}/password")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> SetPassword(Guid id, [FromBody] SetPasswordModel model)
    {
        User user = await _dispatcher.DispatchAsync(new GetUserQuery { Id = id });

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var rs = await _userManager.ResetPasswordAsync(user, token, model.Password);

        if (rs.Succeeded)
        {
            return Ok();
        }

        return BadRequest(rs.Errors);
    }

   // [Authorize(AuthorizationPolicyNames.DeleteUserPolicy)]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var user = await _dispatcher.DispatchAsync(new GetUserQuery { Id = id });
        await _dispatcher.DispatchAsync(new DeleteUserCommand { User = user });

        return Ok();
    }

  //  [Authorize(AuthorizationPolicyNames.SendResetPasswordEmailPolicy)]
    [MapToApiVersion("1.0")]
    [HttpPost("{id}/passwordresetemail")]
    public async Task<ActionResult> SendResetPasswordEmail(Guid id)
    {
        User user = await _dispatcher.DispatchAsync(new GetUserQuery { Id = id });

        if (user != null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetUrl = $"{_appSettings.IdentityServerAuthentication.Authority}/Account/ResetPassword?token={HttpUtility.UrlEncode(token)}&email={user.Email}";

            await _dispatcher.DispatchAsync(new AddOrUpdateEntityCommand<EmailMessage>(new EmailMessage
            {
                From = "phong@gmail.com",
                Tos = user.Email,
                Subject = "Forgot Password",
                Body = $"Reset Url: {resetUrl}"
            }));
        }
        else
        {
            // email user and inform them that they do not have an account
        }

        return Ok();
    }

   // [Authorize(AuthorizationPolicyNames.SendConfirmationEmailAddressEmailPolicy)]
    [HttpPost("{id}/emailaddressconfirmation")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> SendConfirmationEmailAddressEmail(Guid id)
    {
        User user = await _dispatcher.DispatchAsync(new GetUserQuery { Id = id });

        if (user != null)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmationEmail = $"{_appSettings.IdentityServerAuthentication.Authority}/Account/ConfirmEmailAddress?token={HttpUtility.UrlEncode(token)}&email={user.Email}";

            await _dispatcher.DispatchAsync(new AddOrUpdateEntityCommand<EmailMessage>(new EmailMessage
            {
                From = "phong@gmail.com",
                Tos = user.Email,
                Subject = "Confirmation Email",
                Body = $"Confirmation Email: {confirmationEmail}"
            }));
        }
        else
        {
            // email user and inform them that they do not have an account
        }

        return Ok();
    }
}