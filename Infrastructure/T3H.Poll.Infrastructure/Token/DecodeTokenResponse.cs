using ClaimTypes = T3H.Poll.Infrastructure.Token.ClaimTypes;

namespace T3H.Poll.Application.Common.DTOs;

public class DecodeTokenResponse
{
    [JsonPropertyName("sub")]
    public string? Sub { get; set; }

    [JsonPropertyName(ClaimTypes.TokenFamilyId)]
    public string? FamilyId { get; set; }
}
