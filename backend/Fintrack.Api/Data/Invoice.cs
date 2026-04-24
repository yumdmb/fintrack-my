namespace Fintrack.Api.Data;

public sealed class Invoice
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public string InvoiceNumber { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string? CustomerRegistrationNumber { get; set; }

    public string? CustomerTaxIdentificationNumber { get; set; }

    public DateOnly IssueDate { get; set; }

    public DateOnly? DueDate { get; set; }

    public string CurrencyCode { get; set; } = "MYR";

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    public decimal Subtotal { get; set; }

    public decimal TaxTotal { get; set; }

    public decimal GrandTotal { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<InvoiceLineItem> LineItems { get; } = [];
}
