namespace Fintrack.Api.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "Fintrack.Api";

    public string Audience { get; init; } = "Fintrack.Frontend";

    public string SigningKey { get; init; } = string.Empty;

    public int AccessTokenMinutes { get; init; } = 60;
}
