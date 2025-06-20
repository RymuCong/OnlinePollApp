using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using T3H.Poll.CrossCuttingConcerns.ExtensionMethods;
using T3H.Poll.Infrastructure.Web.ExceptionHandlers;
using T3H.Poll.WebApi;
using StackExchange.Redis;
using T3H.Poll.Persistence;
using T3H.Poll.Infrastructure.DateTimes;
using T3H.Poll.Infrastructure.Interceptors;
using T3H.Poll.Infrastructure.Logging;
using T3H.Poll.Infrastructure.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using T3H.Poll.Application.Common.Mapping;
using T3H.Poll.Infrastructure;
using T3H.Poll.Infrastructure.Caching;
using T3H.Poll.Infrastructure.Web.Authentication;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;


var appSettings = new AppSettings();
configuration.Bind(appSettings);

builder.WebHost.UseCRMLogger(configuration =>
{
    return appSettings.Logging;
});


services.AddCors(options =>
{
    options.AddPolicy("CRM", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

services.AddHttpContextAccessor();
services.Configure<AppSettings>(configuration);
services.AddExceptionHandler<GlobalExceptionHandler>();

services.AddControllers(configure =>
{
})
.ConfigureApiBehaviorOptions(options =>
{
})
.AddJsonOptions(options =>
{
});
services.AddSwagger(typeof(ApiAnchor));

services.AddPersistence(appSettings.ConnectionStrings.CRMDatabase)
        .AddMessageHandlers()
        .AddApplicationServices((Type serviceType, Type implementationType, ServiceLifetime serviceLifetime) =>
        {
            services.AddInterceptors(serviceType, implementationType, serviceLifetime, appSettings.Interceptors);
        })
        .ConfigureInterceptors();

services.AddDataProtection()
    .SetApplicationName("T3H.Poll");
services.AddDetection();

services.AddIdentityCore();

services.AddInfrastructureDependencies(configuration, builder.Environment.EnvironmentName);

services.AddDateTimeProvider();

var mapperConfig = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<MappingModel>();
});
services.AddSingleton(mapperConfig.CreateMapper().RegisterMap());
services.AddSingleton(sp => sp.GetRequiredService<IMapper>().ConfigurationProvider);

services.AddAuthentication(options =>
{
    options.DefaultScheme = appSettings.Authentication.Provider switch
    {
        "Jwt" => "Jwt",
        _ => JwtBearerDefaults.AuthenticationScheme
    };
})
.AddJwtBearer("Jwt", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = appSettings.Authentication.Jwt.IssuerUri,
        ValidAudience = appSettings.Authentication.Jwt.Audience,
        TokenDecryptionKey = new X509SecurityKey(appSettings.Authentication.Jwt.TokenDecryptionCertificate.FindCertificate()),
        IssuerSigningKey = new X509SecurityKey(appSettings.Authentication.Jwt.IssuerSigningCertificate.FindCertificate()),
    };
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
{
    var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost";
    return ConnectionMultiplexer.Connect(redisConnectionString);
});

builder.Services.AddSingleton<RedisCacheService>();

var app = builder.Build();

app.UseCors("CRM");

app.UseExceptionHandler(options => { });
app.UseSwagger();
app.UseRouting();
app.MapGet("/", () => Results.LocalRedirect("~/swagger"));
HttpContextCustom.SetupHttpContext(app);

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
// Configure the HTTP request pipeline.


app.Run();


