namespace Fintrack.Api.Invoices;

public sealed class InvoiceExportValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("The invoice does not meet the export requirements.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
