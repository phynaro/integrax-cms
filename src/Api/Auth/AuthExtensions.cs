using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Api.Auth;

public static class AuthExtensions
{
    public static IServiceCollection AddKeycloakAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var keycloakSettings = configuration.GetSection(KeycloakSettings.SectionName).Get<KeycloakSettings>()
            ?? new KeycloakSettings();

        services.Configure<KeycloakSettings>(configuration.GetSection(KeycloakSettings.SectionName));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = keycloakSettings.Authority;
            options.RequireHttpsMetadata = keycloakSettings.RequireHttpsMetadata;
            options.Audience = keycloakSettings.ClientId;
            options.MapInboundClaims = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = keycloakSettings.ValidateIssuer,
                ValidateAudience = keycloakSettings.ValidateAudience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                NameClaimType = "preferred_username",
                RoleClaimType = "role",
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<JwtBearerHandler>>();
                    logger.LogError(context.Exception, "Authentication failed");
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<JwtBearerHandler>>();
                    var userId = context.Principal?.FindFirst("sub")?.Value;
                    logger.LogDebug("Token validated for user {UserId}", userId);
                    return Task.CompletedTask;
                }
            };
        });

        services.AddScoped<IClaimsTransformation, JwtClaimsTransformer>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IUserSyncService, UserSyncService>();

        return services;
    }

    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(AuthConstants.Policies.RequireAuthenticated, policy =>
                policy.RequireAuthenticatedUser())
            .AddPolicy(AuthConstants.Policies.RequireSystemAdmin, policy =>
                policy.RequireRole(AuthConstants.Roles.SystemAdmin))
            .AddPolicy(AuthConstants.Policies.RequireManagerOrAbove, policy =>
                policy.RequireRole(AuthConstants.Roles.SystemAdmin, AuthConstants.Roles.Manager));

        return services;
    }
}
