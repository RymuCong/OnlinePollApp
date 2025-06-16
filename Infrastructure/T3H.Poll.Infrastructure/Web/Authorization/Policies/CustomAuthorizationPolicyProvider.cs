using T3H.Poll.Infrastructure.Web.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace T3H.Poll.Infrastructure.Web.Authorization.Policies;

internal class CustomAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public CustomAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    public override Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith("Permission:", StringComparison.InvariantCultureIgnoreCase))
        {
            var policyBuilder = new AuthorizationPolicyBuilder();

            policyBuilder.RequireAuthenticatedUser();

            policyBuilder.AddRequirements(new PermissionRequirement
            {
                PermissionName = policyName
            });

            var policy = policyBuilder.Build();

            return Task.FromResult(policy);
        }

        return base.GetPolicyAsync(policyName);
    }
}
