namespace T3H.Poll.Infrastructure.Token;

public class JwtSettings
{
    public string SecretKey { get; set; }

    public string ExpireTimeAccessToken { get; set; }

    public string ExpireTimeRefreshToken { get; set; }
}
