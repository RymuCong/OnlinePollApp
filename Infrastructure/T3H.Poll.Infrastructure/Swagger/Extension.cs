
using FDS.CRM.Infrastructure.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.PlatformAbstractions;

namespace FDS.CRM.Infrastructure.Swagger;

public static class Extentions
{
    public static IServiceCollection AddSwagger(this IServiceCollection services, Type anchor)
    {
        //services.AddApiVersioning(
        //    options =>
        //    {
        //        options.ReportApiVersions = true;
        //    });
        //services.AddVersionedApiExplorer(
        //    options =>
        //    {
        //        options.GroupNameFormat = "'v'VVV";

        //        options.SubstituteApiVersionInUrl = true;
        //    });
        //services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        //services.AddSwaggerGen(
        //    options =>
        //    {
        //        options.OperationFilter<SwaggerDefaultValues>();

        //        var xmlFile = XmlCommentsFilePath(anchor);
        //        if (File.Exists(xmlFile))
        //        {
        //            options.IncludeXmlComments(xmlFile);
        //        }
        //    });

        //return services;

        static string XmlCommentsFilePath(Type anchor)
        {
            var basePath = PlatformServices.Default.Application.ApplicationBasePath;
            var fileName = anchor.GetTypeInfo().Assembly.GetName().Name + ".xml";
            return Path.Combine(basePath, fileName);
        }

        services.AddApiVersioning(config =>
        {
            config.DefaultApiVersion = new ApiVersion(1, 0);
            config.ReportApiVersions = true;
            config.AssumeDefaultVersionWhenUnspecified = true;
            config.ApiVersionReader = new QueryStringApiVersionReader("api-version");
            SwaggerConfig.UseQueryStringApiVersion("api-version");


            //config.ApiVersionReader = new HeaderApiVersionReader("X-Version");
            //SwaggerConfig.UseCustomHeaderApiVersion("X-Version");

            //config.ApiVersionReader = new MediaTypeApiVersionReader("v");
            //SwaggerConfig.UseAcceptHeaderApiVersion("v");

        });
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1.0", new OpenApiInfo()
            {
                Title = "Digital Platform version 1.0",
                Version = "v1.0",
                Description = "Digital Platform Api version 1.0",
            });
            options.SwaggerDoc("v2.0", new OpenApiInfo()
            {
                Title = "Digital Insurance Configuration version 2.0",
                Version = "v2.0",
                Description = "Digital Platform Api version 2",
            });

            /// options.OperationFilter<AddAcceptHeaderParameter>();
            options.OperationFilter<SwaggerParameterFilters>();
            options.DocumentFilter<SwaggerVersionMapping>();
            options.OperationFilter<FileUploadFilter>();

            var xmlFile = XmlCommentsFilePath(anchor);
            if (File.Exists(xmlFile))
            {
                options.IncludeXmlComments(xmlFile);
            }

            options.DocInclusionPredicate((version, desc) =>
            {
                if (!desc.TryGetMethodInfo(out MethodInfo methodInfo)) return false;
                var versions = methodInfo.DeclaringType.GetCustomAttributes(true).OfType<ApiVersionAttribute>().SelectMany(attr => attr.Versions);
                var maps = methodInfo.GetCustomAttributes(true).OfType<MapToApiVersionAttribute>().SelectMany(attr => attr.Versions).ToArray();
                version = version.Replace("v", "");
                return versions.Any(v => v.ToString() == version && maps.Any(v => v.ToString() == version));
            });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                                  \r\n\r\nExample: 'Bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            options.OperationFilter<FileUploadFilter>();
            options.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,

                    },
                    new List<string>()
                }
            });
        });

        return services;
    }

    public static IApplicationBuilder UseSwagger(this IApplicationBuilder app)
    {
        app.UseSwagger(options => options.RouteTemplate = "swagger/{documentName}/swagger.json");
        app.UseSwaggerUI(options =>
        {
            //options.RoutePrefix = "";
            options.DefaultModelsExpandDepth(-1);
            options.DocumentTitle = "Digital Insurance Configuration Api";
            options.SwaggerEndpoint($"/swagger/v1.0/swagger.json", $"v1");
            options.SwaggerEndpoint($"/swagger/v2.0/swagger.json", $"v2");
        });
        return app;
    }
}
