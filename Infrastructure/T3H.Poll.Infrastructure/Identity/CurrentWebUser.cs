namespace T3H.Poll.Infrastructure.Identity;

public class CurrentWebUser : ICurrentUser
{
    private readonly IHttpContextAccessor _context;
    private ClaimsPrincipal user = null!;

    public CurrentWebUser(IHttpContextAccessor context)
    {
        _context = context;
    }

    public bool IsAuthenticated
    {
        get
        {
            return _context.HttpContext.User.Identity.IsAuthenticated;
        }
    }

    public Guid UserId
    {
        get
        {
            var userId = _context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                         ?? _context.HttpContext.User.FindFirst("sub")?.Value;

            return Guid.Parse(userId);
        }
    }

    public string Email
    {
        get
        {
            return _context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                   ?? _context.HttpContext.User.FindFirst("email")?.Value;
        }
    }

    public string GetClientIp(HttpContext httpContext)
    {
        return httpContext.Connection.RemoteIpAddress?.ToString();
    }
}
