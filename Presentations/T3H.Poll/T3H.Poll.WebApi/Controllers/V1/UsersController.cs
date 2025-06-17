using T3H.Poll.Application.Common.Commands;
using T3H.Poll.Application.Users.Commands;
using T3H.Poll.Application.Users.Queries;
using T3H.Poll.CrossCuttingConcerns.ExtensionMethods;
using T3H.Poll.WebApi.Models.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using T3H.Poll.Application.Common.Token;
using T3H.Poll.Application.Users.Services;
using T3H.Poll.Domain.Identity;
using T3H.Poll.Infrastructure.Caching;
using Wangkanai.Detection.Services;

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
    
    // Cung cấp các hàm để quản lý người dùng Tạo, xoá, cập nhật, tìm kiếm, đổi mật khẩu, Khoá tài khoản, xác minh email
    private readonly UserManager<User> _userManager;
    // Cung cấp chức năng về xử lý đăng nhập, đăng xuất người dùng
    private readonly SignInManager<User> _signInManager;
    // Quản lý role (tạo, sửa, xoá, gán quyền cho role)
    //private readonly RoleManager<Role> _roleManager;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ITokenFactory _tokenFactory;
    private readonly AppSettings _appSettings;
    private readonly IDetectionService _detectionService;
    private readonly ICurrentUser currentUser;
    private readonly IUserService _userService;
    private readonly RedisCacheService _cache;

    public UsersController(Dispatcher dispatcher,
        UserManager<User> userManager,
        ILogger<UsersController> logger,
        IDateTimeProvider dateTimeProvider,
        IOptionsSnapshot<AppSettings> appSettings,
        SignInManager<User> signInManager,
        ITokenFactory tokenFactory,
        IDetectionService detectionService,
        ICurrentUser currentUser,
        IUserService userService,
        RedisCacheService cache)
    {
        _dispatcher = dispatcher;
        _userManager = userManager;
        _dateTimeProvider = dateTimeProvider;
        _appSettings = appSettings.Value;
        _signInManager = signInManager;
        _tokenFactory = tokenFactory;
        _detectionService = detectionService;
        this.currentUser = currentUser;
        _userService = userService;
        _cache = cache;
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
    
    [HttpPost("login")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> Login([FromBody] LoginModel model, CancellationToken cancellationToken)
    {
        try
        {
            // Kiểm tra user tồn tại
            var user = await _userManager.FindByNameAsync(model.userName);
            if (user == null)
                return BadRequest(new { Message = "Tài khoản không tồn tại" });

            // Kiểm tra password
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
                return BadRequest(new { Message = "Sai mật khẩu" });

            // Lấy roles của user
            var roles = await _userManager.GetRolesAsync(user);
           

            // Lấy thời gian hiện tại theo UTC
            var issuedAtUtc = DateTimeOffset.UtcNow;
            // Chuyển sang Unix timestamp (giây)
            var iatUnixTimestamp = issuedAtUtc.ToUnixTimeSeconds();
            // lay permission
            var per = await _userService.GetPermissionsAsync(user.Id, "Permission");

            var cacheKey = $"permissions/{user.Id}/{iatUnixTimestamp}";

            // Lưu vào cache
            await _cache.SetAsync(
                 key: cacheKey,
                 value: per,
                 TimeSpan.FromMinutes(30));

            // Tạo token
            DateTime refreshExpireTime;
            DateTime accessTokenExpiredTime;
            string familyId = StringHelpers.GetRandomString(32);
            // Lấy thông tin của trình duyệt (cái nơi call api)
            var userAgent = _detectionService.UserAgent.ToString();

            if (model.RememberMe)
            {
                accessTokenExpiredTime = DateTime.UtcNow.AddDays(7);
                refreshExpireTime = DateTime.UtcNow.AddDays(30);
            }
            else
            {
                accessTokenExpiredTime = _tokenFactory.AccesstokenExpiredTime;
                refreshExpireTime = _tokenFactory.RefreshtokenExpiredTime;
            }

            var userToken = new UserAccessToken()
            {
                ExpiredTime = refreshExpireTime,
                UserId = user.Id,
                FamilyId = familyId,
                UserAgent = userAgent,
                // ClientIp = currentUser.GetClientIp(),
            };

            string accessToken = _tokenFactory.CreateToken(
                [
                    new(JwtRegisteredClaimNames.Sub.ToString(), user.Id.ToString()),
                    new Claim(System.Security.Claims.ClaimTypes.Role, roles.FirstOrDefault()),
                    new Claim(System.Security.Claims.ClaimTypes.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Iat, iatUnixTimestamp.ToString(), ClaimValueTypes.Integer64),
                 ], accessTokenExpiredTime);

            string refreshToken = _tokenFactory.CreateToken(
                [
                    new(Infrastructure.Token.ClaimTypes.TokenFamilyId, familyId),
                    new(JwtRegisteredClaimNames.Sub, user.Id.ToString())

                ], refreshExpireTime);

            userToken.RefreshToken = refreshToken;

            Response.Cookies.Append("access_token", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Bắt buộc dùng HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = accessTokenExpiredTime
            });

            Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Bắt buộc dùng HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = refreshExpireTime
            });

            //await unitOfWork.Repository<UserAccessToken>().AddAsync(userToken, cancellationToken);
            //await unitOfWork.SaveAsync(cancellationToken);
            //return Ok();
            var data = new
            {
                Token = accessToken,
                Refresh = refreshToken,
                AccessTokenExpiredIn = (long)
                    Math.Ceiling((accessTokenExpiredTime - DateTime.UtcNow).TotalSeconds),
                TokenType = JwtBearerDefaults.AuthenticationScheme,
                User = new
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Roles = roles,
                }
            };
            var result1 = new ResultModel<object>(data);
            return Ok(result1);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Lỗi server", Error = ex.Message });
        }

    }
}