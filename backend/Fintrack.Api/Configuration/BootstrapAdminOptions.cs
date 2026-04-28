namespace Fintrack.Api.Configuration;

public sealed class BootstrapAdminOptions
{
    public const string SectionName = "BootstrapAdmin";

    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string CompanyName { get; init; } = string.Empty;

    public string RegistrationNumber { get; init; } = string.Empty;

    public string TaxIdentificationNumber { get; init; } = string.Empty;

    public string? SalesAndServiceTaxNumber { get; init; }

    public string DefaultCurrencyCode { get; init; } = "MYR";
}
