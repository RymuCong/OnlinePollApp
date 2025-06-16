using T3H.Poll.Infrastructure.Caching;
using ClaimTypes = System.Security.Claims.ClaimTypes;


namespace T3H.Poll.Infrastructure.Web.Authorization.ClaimsTransformations;

public class CustomClaimsTransformation : IClaimsTransformation
{
    private readonly RedisCacheService _cache;

    public CustomClaimsTransformation(RedisCacheService cache)
    {
        _cache = cache;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = principal.Identities.FirstOrDefault(x => x.IsAuthenticated);
        if (identity == null)
        {
            return principal;
        }

        var userClaim = principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

        if (Guid.TryParse(userClaim?.Value, out var userId))
        {
            var issuedAt = principal.Claims.FirstOrDefault(x => x.Type == "iat").Value;


            var cacheKey = $"permissions/{userId}/{issuedAt}";

            var permissions = await _cache.GetOrCreateAsync(cacheKey, async () => await GetPermissionsAsync(userId));

            var claims = new List<Claim>();
            claims.AddRange(permissions.Select(p => new Claim("Permission", p)));
            claims.AddRange(principal.Claims);

            var newIdentity = new ClaimsIdentity(claims, identity.AuthenticationType);
            return new ClaimsPrincipal(newIdentity);
        }

        return principal;
    }

    private Task<List<string>> GetPermissionsAsync(Guid userId)
    {
        // TODO: Get from Db
        var claims = new List<string>();
        return Task.FromResult(claims);
    }
}
