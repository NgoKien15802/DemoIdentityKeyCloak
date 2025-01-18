using DemoIdentity.Data;
using DemoIdentity.Security.ClaimsTransformation;
using DemoIdentity.Security.PolicyDynamic;
using DemoIdentity.Security.Requirements;
using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DemoIdentity
{
    public static class ConfigurationService
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddAuthentication(configuration)
                .AddSystemAuthorization()
                .AddApplicationDbContext(configuration);
        }

        private static IServiceCollection AddAuthentication(this IServiceCollection serviceProvider, IConfiguration configuration)
        {
            // add config keycloak
            serviceProvider.AddKeycloakWebApiAuthentication(configuration, options =>
            {
                options.RequireHttpsMetadata = false;

                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true, // Kiểm tra Issuer (ai phát hành token)
                    ValidateAudience = true, // Kiểm tra Audience (ứng dụng nào nhận token)
                    ValidateLifetime = true, // Kiểm tra thời gian hết hạn (exp)
                    ValidateIssuerSigningKey = true, // Bật kiểm tra chữ ký
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Token validation failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine($"Token validated for: {context.Principal.Identity.Name}");
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        Console.WriteLine(context.Token);
                        //context.Token = context.Request.Cookies["X-Access-Token"];
                        return Task.CompletedTask;
                    }
                };
            });
            return serviceProvider; 
        }

        private static IServiceCollection AddSystemAuthorization(this IServiceCollection serviceProvider)
        {

            serviceProvider
                .AddAuthorization()
                // add các app setting cho việc authorize: VD ResourceAccessDefaultClientFromAppSetting ko cần thêm resource mà chỉ thêm rule, resource đọc từ app setting
                //.AddKeycloakAuthorization(configuration)
                .AddKeycloakAuthorization(options =>
                {
                    options.EnableRolesMapping =
                      RolesClaimTransformationSource.ResourceAccess;
                    options.RolesResource = "weather-api";
                })
                .AddAuthorizationBuilder()
               /* .AddPolicy("CanReadWeathers", policy =>
                {
                    policy.Requirements.Add(new PermissionRequirement("ReadWeathers")); // Replace with a dynamic value later
                })*/
                .AddPolicy(
                    "RealmAccess",
                    policy => policy.RequireRealmRoles(["admin-realm"])
                )
                .AddPolicy(
                    "ResourceAccess",
                    policy => policy.RequireResourceRolesForClient("weather-api", ["admin"])
                )
                .AddPolicy(
                    "ResourceAccessDefaultClientFromAppSetting",
                    policy => policy.RequireResourceRoles(["admin"])
                ).AddPolicy(
                    "roleAspDotNet",
                    policy => policy.RequireRole(["admin"])
                );

            serviceProvider.AddScoped<IClaimsTransformation, CustomClaimsTransformation>();
            serviceProvider.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
            serviceProvider.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

            return serviceProvider;
        }

        private static IServiceCollection AddApplicationDbContext(this IServiceCollection serviceProvider, IConfiguration configuration)
        {

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            serviceProvider.AddDbContext<ApplicationDbContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
            return serviceProvider;
        }
    }
}
