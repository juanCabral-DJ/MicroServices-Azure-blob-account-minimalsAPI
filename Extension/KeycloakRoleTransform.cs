using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace minimalAPI.Extension
{
    public class KeycloakRoleTransform : IClaimsTransformation
    {
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var identity = (ClaimsIdentity)principal.Identity;

            var roleClaims = identity.FindFirst("resource_access");

            if (roleClaims != null)
            {
                var resourceAccessJson = System.Text.Json.JsonDocument.Parse(roleClaims.Value);
                if (resourceAccessJson.RootElement.TryGetProperty("document-services", out var clientRoles))
                {
                    if (clientRoles.TryGetProperty("roles", out var roles))
                    {
                        foreach (var role in roles.EnumerateArray())
                        {
                            var roleName = role.GetString();
                            if (!string.IsNullOrEmpty(roleName))
                            {
                                identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                            }
                        }
                    }
                }
            }
            return Task.FromResult(principal);
        }
    }
}
