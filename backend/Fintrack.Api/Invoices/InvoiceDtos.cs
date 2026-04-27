using System.ComponentModel.DataAnnotations;
using Fintrack.Api.Finance;

namespace Fintrack.Api.Invoices;

public sealed class UpsertInvoiceRequest : IValidatableObject
{
    [Required, StringLength(64)]
    public string InvoiceNumber { get; init; } = string.Empty;

    [Required, StringLength(200)]
    public string CustomerName { get; init; } = string.Empty;

    [StringLength(64)]
    public string? CustomerRegistrationNumber { get; init; }

    [StringLength(64)]
    public string? CustomerTaxIdentificationNumber { get; init; }

    public DateOnly IssueDate { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public DateOnly? DueDate { get; init; }

    [StringLength(3, MinimumLength = 3)]
    public string? CurrencyCode { get; init; }

    [Required, MinLength(1)]
    public IReadOnlyCollection<UpsertInvoiceLineItemRequest> LineItems { get; init; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DueDate is not null && DueDate < IssueDate)
        {
            yield return new ValidationResult(
                "Due date cannot be before issue date.",
                [nameof(DueDate)]);
        }
    }
}

public sealed class UpsertInvoiceLineItemRequest : IValidatableObject
{
    [Required, StringLength(300)]
    public string Description { get; init; } = string.Empty;

    [Range(typeof(decimal), "0.0001", "999999999999.9999")]
    public decimal Quantity { get; init; }

    [Range(typeof(decimal), "0", "999999999999.99")]
    public decimal UnitPrice { get; init; }

    [Range(typeof(decimal), "0", "100")]
    public decimal TaxRate { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!FinanceValueRules.HasMaxScale(Quantity, 4))
        {
            yield return new ValidationResult(
                "Quantity cannot contain more than 4 decimal places.",
                [nameof(Quantity)]);
        }

        if (!FinanceValueRules.HasMaxScale(UnitPrice, 2))
        {
            yield return new ValidationResult(
                "Unit price cannot contain more than 2 decimal places.",
                [nameof(UnitPrice)]);
        }

        if (!FinanceValueRules.HasMaxScale(TaxRate, 4))
        {
            yield return new ValidationResult(
                "Tax rate cannot contain more than 4 decimal places.",
                [nameof(TaxRate)]);
        }
    }
}

public sealed record InvoiceSummaryResponse(
    Guid Id,
    string InvoiceNumber,
    string CustomerName,
    DateOnly IssueDate,
    DateOnly? DueDate,
    string CurrencyCode,
    string Status,
    decimal Subtotal,
    decimal TaxTotal,
    decimal GrandTotal);

public sealed record InvoiceResponse(
    Guid Id,
    Guid CompanyId,
    string InvoiceNumber,
    string CustomerName,
    string? CustomerRegistrationNumber,
    string? CustomerTaxIdentificationNumber,
    DateOnly IssueDate,
    DateOnly? DueDate,
    string CurrencyCode,
    string Status,
    decimal Subtotal,
    decimal TaxTotal,
    decimal GrandTotal,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<InvoiceLineItemResponse> LineItems);

public sealed record InvoiceLineItemResponse(
    Guid Id,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate,
    decimal LineSubtotal,
    decimal TaxAmount,
    decimal LineTotal);
