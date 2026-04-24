namespace Fintrack.Api.Invoices;

public sealed class DuplicateInvoiceNumberException(string invoiceNumber)
    : InvalidOperationException($"Invoice number '{invoiceNumber}' already exists for this company.")
{
    public string InvoiceNumber { get; } = invoiceNumber;
}
