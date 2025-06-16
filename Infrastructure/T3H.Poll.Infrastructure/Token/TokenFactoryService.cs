
using T3H.Poll.Application.Common.DTOs;
using T3H.Poll.Application.Common.Token;

namespace T3H.Poll.Infrastructure.Token;

public class TokenFactoryService(IOptions<JwtSettings> jwtSettings) : ITokenFactory
{
    private readonly JwtSettings settings = jwtSettings.Value;

    public DateTime AccesstokenExpiredTime => GetAccesstokenExpiredTime();

    public DateTime RefreshtokenExpiredTime => GetRefreshtokenExpiredTime();

    // Tạo JWT Token
    public string CreateToken(IEnumerable<Claim> claims, DateTime expirationTime)
    {
        return JwtBuilder
            .Create()
            .WithAlgorithm(new HMACSHA256Algorithm())
            .AddClaims(claims.Select(x => new KeyValuePair<string, object>(x.Type, x.Value)))
            .WithSecret(settings.SecretKey)
            .ExpirationTime(expirationTime)
            .WithValidationParameters(
                new ValidationParameters() { ValidateExpirationTime = true, TimeMargin = 0, ValidateSignature = true}
            )
            .Encode();
    }

    public DecodeTokenResponse DecodeToken(string token)
    {
        var json = JwtBuilder
            .Create()
            .WithAlgorithm(new HMACSHA256Algorithm())
            .WithSecret(settings.SecretKey)
            .MustVerifySignature()
            .Decode(token);

        return SerializerExtension.Deserialize<DecodeTokenResponse>(json).Object!;
    }

    private DateTime GetAccesstokenExpiredTime() =>
        DateTime.UtcNow.AddHours(double.Parse(settings.ExpireTimeAccessToken!));

    private DateTime GetRefreshtokenExpiredTime() =>
        DateTime.UtcNow.AddDays(double.Parse(settings.ExpireTimeRefreshToken!));
}
