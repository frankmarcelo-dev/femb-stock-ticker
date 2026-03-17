using System.Security.Claims;
using FembStockTicker.Config;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace FembStockTicker.Auth0
{
    public static class Auth0ServiceExtensions
    {
        public static IServiceCollection AddAuth0Authentication(
            this IServiceCollection services,
            Auth0Configuration config)
        {
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.Authority = config.Authority;
                    options.Audience = config.Audience;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = config.Authority,
                        ValidateAudience = true,
                        ValidAudience = config.Audience,
                        ValidateLifetime = true,
                        NameClaimType = ClaimTypes.NameIdentifier
                    };
                });

            return services;
        }

        public static IServiceCollection AddAuth0Authorization(
            this IServiceCollection services,
            Auth0Configuration config)
        {
            services.AddAuthorizationBuilder()
                .AddPolicy("read:stocks", policy =>
                    policy.RequireClaim("scope", "read:stocks"))
                .AddPolicy("write:stocks", policy =>
                    policy.RequireClaim("scope", "write:stocks"));

            return services;
        }
    }
}
