namespace T3H.Poll.Infrastructure.Web.Authorization;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services, Assembly assembly)
    {
        services.AddSingleton<IClaimsTransformation, CustomClaimsTransformation>();

        services.AddSingleton<IAuthorizationPolicyProvider, CustomAuthorizationPolicyProvider>();

        services.Configure<AuthorizationOptions>(options =>
        {
        });

        services.AddSingleton<IAuthorizationHandler, PermissionRequirementHandler>();

        var requirementHandlerTypes = assembly.GetTypes()
            .Where(IsAuthorizationHandler)
            .ToList();

        foreach (var type in requirementHandlerTypes)
        {
            services.AddSingleton(typeof(IAuthorizationHandler), type);
        }

        return services;
    }

    private static bool IsAuthorizationHandler(Type type)
    {
        if (type.BaseType == null)
        {
            return false;
        }

        if (!type.BaseType.IsGenericType)
        {
            return false;
        }

        var baseType = type.BaseType.GetGenericTypeDefinition();
        return baseType == typeof(AuthorizationHandler<>) || baseType == typeof(AuthorizationHandler<,>);
    }
}
