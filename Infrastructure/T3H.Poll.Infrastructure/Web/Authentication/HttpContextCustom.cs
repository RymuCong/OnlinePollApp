namespace T3H.Poll.Infrastructure.Web.Authentication;

public static class HttpContextCustom
{
    private static IHttpContextAccessor _httpContextAccessor;

    public static void Configure(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public static HttpContext Current
    {
        get
        {
            return _httpContextAccessor.HttpContext;
        }
    }

    public static void SetupHttpContext(IHost app)
    {
        var scopedFactory = app.Services.GetService<IServiceScopeFactory>();
        if (scopedFactory is not null)
        {
            using var scope = scopedFactory.CreateScope();
            var context = scope.ServiceProvider.GetService<IHttpContextAccessor>();
            if (context is not null)
            {
                Configure(context);
            }
            else
            {
                //Log.Error("Can't seed data because services of DataSeeder not create");
            }
        }
    }
}
