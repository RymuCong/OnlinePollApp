using FDS.CRM.Infrastructure.Web.ExceptionHandlers;
using FDS.CRM.WebApi;
using FDS.CRM.WebApi.ConfigurationOptions;
using FDS.CRM.Persistence;
using FDS.CRM.Infrastructure.DateTimes;
using FDS.CRM.Infrastructure.Interceptors;
using FDS.CRM.Infrastructure.Logging;
using FDS.CRM.Infrastructure.Identity;
using Microsoft.AspNetCore.DataProtection;

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
        //.AddApplicationServices((Type serviceType, Type implementationType, ServiceLifetime serviceLifetime) =>
        //{
        //    services.AddInterceptors(serviceType, implementationType, serviceLifetime, appSettings.Interceptors);
        //})
        .ConfigureInterceptors();

services.AddDataProtection()
    .SetApplicationName("FDS.CRM");

services.AddIdentityCore();

services.AddDateTimeProvider();

//services.AddEndpointsApiExplorer();
//services.AddSwaggerGen(setupAction =>
//{
//    setupAction.SwaggerDoc(
//        $"v1.0",
//        new OpenApiInfo()
//        {
//            Title = " API",
//            Version = "1",
//            Description = "ClassifiedAds API Specification.",
//            Contact = new OpenApiContact
//            {
//                Email = "abc.xyz@gmail.com",
//                Name = "Phong Nguyen",
//                Url = new Uri("https://github.com/phongnguyend"),
//            },
//            License = new OpenApiLicense
//            {
//                Name = "MIT License",
//                Url = new Uri("https://opensource.org/licenses/MIT"),
//            },
//        });

//    setupAction.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
//    {
//        Type = SecuritySchemeType.Http,
//        Scheme = "Bearer",
//        BearerFormat = "JWT",
//        Description = "Input your Bearer token to access this API",
//    });


//    setupAction.AddSecurityDefinition("Oidc", new OpenApiSecurityScheme
//    {
//        Type = SecuritySchemeType.OAuth2,
//        Flows = new OpenApiOAuthFlows
//        {
//            AuthorizationCode = new OpenApiOAuthFlow
//            {
//                //TokenUrl = new Uri(appSettings.IdentityServerAuthentication.Authority + "/connect/token", UriKind.Absolute),
//                //AuthorizationUrl = new Uri(appSettings.IdentityServerAuthentication.Authority + "/connect/authorize", UriKind.Absolute),
//                //Scopes = new Dictionary<string, string>
//                //{
//                //            { "openid", "OpenId" },
//                //            { "profile", "Profile" },
//                //            { "ClassifiedAds.WebAPI", "ClassifiedAds WebAPI" },
//                //},
//            },
//            ClientCredentials = new OpenApiOAuthFlow
//            {
//                //TokenUrl = new Uri(appSettings.IdentityServerAuthentication.Authority + "/connect/token", UriKind.Absolute),
//                //Scopes = new Dictionary<string, string>
//                //{
//                //            { "ClassifiedAds.WebAPI", "ClassifiedAds WebAPI" },
//                //},
//            },
//        },
//    });

//    setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference
//                {
//                    Type = ReferenceType.SecurityScheme,
//                    Id = "Oidc",
//                },
//            }, new List<string>()
//        },
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference
//                {
//                    Type = ReferenceType.SecurityScheme,
//                    Id = "Bearer",
//                },
//            }, new List<string>()
//        },
//    });
//});

var app = builder.Build();

app.UseCors("CRM");

app.UseExceptionHandler(options => { });
app.UseSwagger();
app.UseRouting();
app.MapGet("/", () => Results.LocalRedirect("~/swagger"));
//app.UseSwaggerUI(setupAction =>
//{
//    setupAction.OAuthClientId("Swagger");
//    setupAction.OAuthClientSecret("secret");
//    setupAction.OAuthUsePkce();

//    setupAction.SwaggerEndpoint(
//        "/swagger/ClassifiedAds/swagger.json",
//        "ClassifiedAds API");

//    setupAction.RoutePrefix = string.Empty;

//    setupAction.DefaultModelExpandDepth(2);
//    setupAction.DefaultModelRendering(ModelRendering.Model);
//    setupAction.DocExpansion(DocExpansion.None);
//    setupAction.EnableDeepLinking();
//    setupAction.DisplayOperationId();
//});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
// Configure the HTTP request pipeline.


app.Run();


