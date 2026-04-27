using System.ComponentModel.DataAnnotations;

namespace Fintrack.Api.Dashboard;

public sealed class DashboardSummaryQuery : IValidatableObject
{
    public DateOnly? StartDate { get; init; }

    public DateOnly? EndDate { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate is not null && EndDate is not null && EndDate < StartDate)
        {
            yield return new ValidationResult(
                "End date cannot be before start date.",
                [nameof(EndDate)]);
        }
    }
}

public sealed class DashboardTrendsQuery : IValidatableObject
{
    public DateOnly? StartMonth { get; init; }

    public DateOnly? EndMonth { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartMonth is not null && EndMonth is not null && EndMonth < StartMonth)
        {
            yield return new ValidationResult(
                "End month cannot be before start month.",
                [nameof(EndMonth)]);
        }
    }
}

public sealed record DashboardSummaryResponse(
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Revenue,
    decimal Expenses,
    int InvoiceCount,
    decimal OutstandingBalance);

public sealed record DashboardTrendsResponse(
    DateOnly StartMonth,
    DateOnly EndMonth,
    IReadOnlyCollection<DashboardTrendBucketResponse> Buckets);

public sealed record DashboardTrendBucketResponse(
    DateOnly MonthStart,
    decimal Revenue,
    decimal Expenses);
