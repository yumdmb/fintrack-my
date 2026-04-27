namespace Fintrack.Api.Invoices;

public sealed record InvoiceJsonExportResponse(
    Guid InvoiceId,
    Guid CompanyId,
    string InvoiceNumber,
    string Status,
    DateOnly IssueDate,
    DateOnly? DueDate,
    string CurrencyCode,
    InvoiceExportPartyResponse Seller,
    InvoiceExportPartyResponse Buyer,
    decimal Subtotal,
    decimal TaxTotal,
    decimal GrandTotal,
    IReadOnlyCollection<InvoiceExportLineItemResponse> LineItems);

public sealed record InvoiceExportPartyResponse(
    string Name,
    string? RegistrationNumber,
    string? TaxIdentificationNumber,
    string? SalesAndServiceTaxNumber);

public sealed record InvoiceExportLineItemResponse(
    Guid Id,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate,
    decimal LineSubtotal,
    decimal TaxAmount,
    decimal LineTotal);
