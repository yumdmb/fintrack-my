namespace Fintrack.Api.Configuration;

public sealed class CorsOptions
{
    public const string PolicyName = "Frontend";
    public const string SectionName = "Cors";

    public List<string> AllowedOrigins { get; init; } = [];
}
