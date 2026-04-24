namespace Fintrack.Api.Data;

public sealed class Company
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string? RegistrationNumber { get; set; }

    public string? TaxIdentificationNumber { get; set; }

    public string? SalesAndServiceTaxNumber { get; set; }

    public string DefaultCurrencyCode { get; set; } = "MYR";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<ApplicationUser> Users { get; } = [];

    public ICollection<Invoice> Invoices { get; } = [];

    public ICollection<Expense> Expenses { get; } = [];
}
