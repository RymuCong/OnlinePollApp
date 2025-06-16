namespace T3H.Poll.Infrastructure.Web.Authorization.Requirements;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string PermissionName { get; set; }
}

public class PermissionRequirementHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var user = context.User;

        if (!user.Identity.IsAuthenticated)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        if (user.HasClaim("Permission", requirement.PermissionName))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
