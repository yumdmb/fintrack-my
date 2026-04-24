namespace Fintrack.Api.Data;

public sealed class InvoiceLineItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid InvoiceId { get; set; }

    public Invoice Invoice { get; set; } = null!;

    public string Description { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TaxRate { get; set; }

    public decimal LineSubtotal { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal LineTotal { get; set; }
}
