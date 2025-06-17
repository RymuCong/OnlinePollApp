namespace T3H.Poll.Infrastructure.DateTimes;

public static class DateTimeProviderExtensions
{
    public static IServiceCollection AddDateTimeProvider(this IServiceCollection services)
    {
        _ = services.AddSingleton<CrossCuttingConcerns.DateTimes.IDateTimeProvider, DateTimeProvider>();
        return services;
    }
}
