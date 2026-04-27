using System.ComponentModel.DataAnnotations;
using Fintrack.Api.Finance;

namespace Fintrack.Api.Expenses;

public sealed class UpsertExpenseRequest : IValidatableObject
{
    public DateOnly ExpenseDate { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow);

    [Required, StringLength(100)]
    public string Category { get; init; } = string.Empty;

    [Required, StringLength(500)]
    public string Description { get; init; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999999999.99")]
    public decimal Amount { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!FinanceValueRules.HasMaxScale(Amount, 2))
        {
            yield return new ValidationResult(
                "Amount cannot contain more than 2 decimal places.",
                [nameof(Amount)]);
        }
    }
}

public sealed record ExpenseSummaryResponse(
    Guid Id,
    DateOnly ExpenseDate,
    string Category,
    string Description,
    decimal Amount,
    DateTimeOffset CreatedAt);

public sealed record ExpenseResponse(
    Guid Id,
    Guid CompanyId,
    DateOnly ExpenseDate,
    string Category,
    string Description,
    decimal Amount,
    Guid? CreatedByUserId,
    DateTimeOffset CreatedAt);
