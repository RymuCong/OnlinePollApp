using T3H.Poll.CrossCuttingConcerns.Common.Interfaces.Registers;

namespace T3H.Poll.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDependencies(
        this IServiceCollection services,
        IConfiguration configuration,
        string? environmentName = "Development"
    )
    {
      // Scrutor giúp giảm thiểu cấu hình thủ công, đặc biệt vs hệ thống lớn nhiều service hoặc repository
        services.Scan(scan =>
                scan.FromCallingAssembly()
                    .AddClasses(classes => classes.AssignableTo<IScope>())  // Tự động tìm các class kế thưa IScope có vòng đời là Scoped
                    .AsImplementedInterfaces()  // Đăng ký tất cả các interface mà class đấy implement
                    .WithScopedLifetime()
                    .AddClasses(classes => classes.AssignableTo<ISingleton>())
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime()
                    .AddClasses(classes => classes.AssignableTo<ITransient>())
                    .AsImplementedInterfaces()
                    .WithTransientLifetime()
            )
            .AddSingleton<IActionContextAccessor, ActionContextAccessor>()
            .AddSingleton<ICurrentUser, CurrentWebUser>()
            .AddJwtAuth(configuration);

        return services;
    }
}
