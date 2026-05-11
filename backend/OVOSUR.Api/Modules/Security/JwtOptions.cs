namespace OVOSUR.Api.Modules.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "OVOSUR.Api";
    public string Audience { get; init; } = "OVOSUR.Web";
    public string SigningKey { get; init; } = "DEV_ONLY_CHANGE_THIS_SIGNING_KEY_32_CHARS_MIN";
    public int AccessTokenMinutes { get; init; } = 480;
    public int RefreshTokenDays { get; init; } = 7;
}
