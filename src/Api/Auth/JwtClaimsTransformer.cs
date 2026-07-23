using System.Security.Claims;
using Identity.Core.Interfaces;
using Microsoft.AspNetCore.Authentication;

namespace Api.Auth;

public class JwtClaimsTransformer : IClaimsTransformation
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<JwtClaimsTransformer> _logger;

    public JwtClaimsTransformer(IServiceProvider serviceProvider, ILogger<JwtClaimsTransformer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
            return principal;

        var keycloakId = principal.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(keycloakId))
            return principal;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            
            var user = await userRepository.GetByKeycloakIdAsync(keycloakId);
            if (user == null)
                return principal;

            var claimsIdentity = principal.Identity as ClaimsIdentity;
            if (claimsIdentity == null)
                return principal;

            claimsIdentity.AddClaim(new Claim("userId", user.Id.ToString()));
            claimsIdentity.AddClaim(new Claim("role", user.Role?.Name ?? AuthConstants.Roles.Viewer));
            
            if (user.Role?.Permissions != null)
            {
                claimsIdentity.AddClaim(new Claim("permissions", string.Join(",", user.Role.Permissions)));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transforming claims for user {KeycloakId}", keycloakId);
        }

        return principal;
    }
}
