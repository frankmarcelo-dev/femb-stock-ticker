using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FembStockTicker.Config;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace FembStockTicker.Swagger
{
    [ExcludeFromCodeCoverage]
    public static class SwaggerConfiguration
    {
        private const string DefaultScheme = "https";
        private const string DefaultBasePath = "/";

        private static SwaggerConfigurationSettings? swaggerOptions;

        private static string ApiName => swaggerOptions!.ApiName;
        private static string ApiVersion => swaggerOptions!.ApiVersion;
        private static string ApiDescription => swaggerOptions!.ApiDescription;
        private static string ApiHost => swaggerOptions!.ApiHost;

        public static void WithSwaggerOptions(SwaggerOptions options)
        {
            options.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
            {
                swaggerDoc.Servers = new List<OpenApiServer>
                {
                    new OpenApiServer
                    {
                        Url = $"{DefaultScheme}://{ApiHost}{DefaultBasePath}"
                    }
                };
            });
        }

        public static void WithSwaggerGenServiceOptions(SwaggerGenOptions options)
        {
            options.DescribeAllParametersInCamelCase();
            options.SwaggerDoc(
                ApiVersion,
                new OpenApiInfo
                {
                    Title = ApiName,
                    Description = ApiDescription,
                    Version = ApiVersion
                }
            );
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "bearer"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type=ReferenceType.SecurityScheme,
                            Id="Bearer"
                        }
                    },
                    new string[]{}
                }
            });
            options.CustomOperationIds(apiDesc =>
            {
                return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
            });
        }
    
        public static void WithSwaggerUiOptions(SwaggerUIOptions options)
        {
            options.SwaggerEndpoint($"/swagger/{ApiVersion}/swagger.json", ApiName);
        }

        public static void ConfigureSwaggerWith(AppConfiguration configuration)
        {
            swaggerOptions = configuration;
        }
    }
}
