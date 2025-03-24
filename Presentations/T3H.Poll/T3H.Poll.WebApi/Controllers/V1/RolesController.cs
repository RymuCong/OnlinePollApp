namespace FDS.CRM.WebApi.Controllers.V1;

[EnableRateLimiting(RateLimiterPolicyNames.DefaultPolicy)]
// [Authorize]
[Produces("application/json")]
[ApiController]
[ApiVersion("1.0")]
[Route("/api/v{version:apiVersion}/[controller]/")]
public class RolesController : ControllerBase
{
    private readonly Dispatcher _dispatcher;

    public RolesController(Dispatcher dispatcher, ILogger<RolesController> logger)
    {
        _dispatcher = dispatcher;
    }

   // [Authorize(AuthorizationPolicyNames.GetRolesPolicy)]
    [MapToApiVersion("1.0")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Role>>> Get()
    {
        var roles = await _dispatcher.DispatchAsync(new GetRolesQuery { AsNoTracking = true });
        var model = roles.ToModels();
        return Ok(model);
    }

   // [Authorize(AuthorizationPolicyNames.GetRolePolicy)]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<Role>> Get(Guid id)
    {
        var role = await _dispatcher.DispatchAsync(new GetRoleQuery { Id = id, AsNoTracking = true });
        var model = role.ToModel();
        return Ok(model);
    }

   // [Authorize(AuthorizationPolicyNames.AddRolePolicy)]
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<Role>> Post([FromBody] RoleModel model)
    {
        var role = new Role
        {
            Name = model.Name,
            NormalizedName = model.Name.ToUpper(),
        };

        await _dispatcher.DispatchAsync(new AddUpdateRoleCommand { Role = role });

        model = role.ToModel();

        return Created($"/api/roles/{model.Id}", model);
    }

   // [Authorize(AuthorizationPolicyNames.UpdateRolePolicy)]
    [HttpPut("{id}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> Put(Guid id, [FromBody] RoleModel model)
    {
        var role = await _dispatcher.DispatchAsync(new GetRoleQuery { Id = id });

        role.Name = model.Name;
        role.NormalizedName = model.Name.ToUpper();

        await _dispatcher.DispatchAsync(new AddUpdateRoleCommand { Role = role });

        model = role.ToModel();

        return Ok(model);
    }

   // [Authorize(AuthorizationPolicyNames.DeleteRolePolicy)]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var role = await _dispatcher.DispatchAsync(new GetRoleQuery { Id = id });
        await _dispatcher.DispatchAsync(new DeleteRoleCommand { Role = role });

        return Ok();
    }
}