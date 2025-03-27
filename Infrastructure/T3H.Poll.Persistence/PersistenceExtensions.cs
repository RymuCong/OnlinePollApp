using Microsoft.AspNetCore.Builder;

namespace T3H.Poll.Persistence;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString, string migrationsAssembly = "")
    {
        services.AddDbContext<CrmDbContext>(options => options.UseSqlServer(connectionString, sql =>
        {
            if (!string.IsNullOrEmpty(migrationsAssembly))
            {
                sql.MigrationsAssembly(migrationsAssembly);
            }
        }))
                .AddDbContextFactory<CrmDbContext>((Action<DbContextOptionsBuilder>)null, ServiceLifetime.Scoped)
                .AddRepositories();

        services.AddScoped(typeof(IDistributedLock), _ => new SqlDistributedLock(connectionString));

        return services;
    }
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Todo: chưa đăng ký repository
        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>))
                .AddScoped(typeof(ICommonSettingRepository), typeof(CommonSettingRepository))
        //.AddScoped(typeof(ICategoryRepository), typeof(CategoryRepository))
        //.AddScoped(typeof(ISupplierRepository), typeof(SupplierRepository));
        .AddScoped(typeof(IUserRepository), typeof(UserRepository))
        .AddScoped(typeof(IRoleRepository), typeof(RoleRepository));

        services.AddScoped(typeof(IUnitOfWork), services =>
        {
            return services.GetRequiredService<CrmDbContext>();
        });

        services.AddScoped<ILockManager, LockManager>();
        // Todo thiếu phần  CircuitBreakerManager
        // services.AddScoped<ICircuitBreakerManager, CircuitBreakerManager>();

        return services;
    }

    public static void MigrateAdsDb(this IApplicationBuilder app)
    {
        using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
        {
            serviceScope.ServiceProvider.GetRequiredService<CrmDbContext>().Database.Migrate();
        }
    }
}
