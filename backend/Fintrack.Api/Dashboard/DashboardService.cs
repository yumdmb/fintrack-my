using Fintrack.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Fintrack.Api.Dashboard;

public sealed class DashboardService(ApplicationDbContext dbContext)
{
    public async Task<DashboardSummaryResponse> GetSummaryAsync(
        Guid companyId,
        DashboardSummaryQuery query,
        CancellationToken cancellationToken = default)
    {
        var (startDate, endDate) = ResolveSummaryRange(query);

        var invoices = await dbContext.Invoices
            .AsNoTracking()
            .Where(invoice => invoice.CompanyId == companyId
                && invoice.IssueDate >= startDate
                && invoice.IssueDate <= endDate)
            .Select(invoice => new
            {
                invoice.Status,
                invoice.GrandTotal
            })
            .ToListAsync(cancellationToken);

        var expenseTotal = await dbContext.Expenses
            .AsNoTracking()
            .Where(expense => expense.CompanyId == companyId
                && expense.ExpenseDate >= startDate
                && expense.ExpenseDate <= endDate)
            .Select(expense => (decimal?)expense.Amount)
            .SumAsync(cancellationToken) ?? 0m;

        var revenue = invoices
            .Where(invoice => invoice.Status == InvoiceStatus.Finalized)
            .Sum(invoice => invoice.GrandTotal);

        return new DashboardSummaryResponse(
            startDate,
            endDate,
            revenue,
            expenseTotal,
            invoices.Count,
            revenue);
    }

    public async Task<DashboardTrendsResponse> GetTrendsAsync(
        Guid companyId,
        DashboardTrendsQuery query,
        CancellationToken cancellationToken = default)
    {
        var (startMonth, endMonth) = ResolveTrendRange(query);
        var endDate = EndOfMonth(endMonth);

        var invoiceRows = await dbContext.Invoices
            .AsNoTracking()
            .Where(invoice => invoice.CompanyId == companyId
                && invoice.Status == InvoiceStatus.Finalized
                && invoice.IssueDate >= startMonth
                && invoice.IssueDate <= endDate)
            .Select(invoice => new
            {
                MonthStart = new DateOnly(invoice.IssueDate.Year, invoice.IssueDate.Month, 1),
                invoice.GrandTotal
            })
            .ToListAsync(cancellationToken);

        var expenseRows = await dbContext.Expenses
            .AsNoTracking()
            .Where(expense => expense.CompanyId == companyId
                && expense.ExpenseDate >= startMonth
                && expense.ExpenseDate <= endDate)
            .Select(expense => new
            {
                MonthStart = new DateOnly(expense.ExpenseDate.Year, expense.ExpenseDate.Month, 1),
                expense.Amount
            })
            .ToListAsync(cancellationToken);

        var invoiceTotals = invoiceRows
            .GroupBy(row => row.MonthStart)
            .ToDictionary(group => group.Key, group => group.Sum(row => row.GrandTotal));

        var expenseTotals = expenseRows
            .GroupBy(row => row.MonthStart)
            .ToDictionary(group => group.Key, group => group.Sum(row => row.Amount));

        var buckets = new List<DashboardTrendBucketResponse>();
        for (var month = startMonth; month <= endMonth; month = month.AddMonths(1))
        {
            invoiceTotals.TryGetValue(month, out var revenue);
            expenseTotals.TryGetValue(month, out var expenses);

            buckets.Add(new DashboardTrendBucketResponse(month, revenue, expenses));
        }

        return new DashboardTrendsResponse(startMonth, endMonth, buckets);
    }

    private static (DateOnly StartDate, DateOnly EndDate) ResolveSummaryRange(DashboardSummaryQuery query)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var currentMonthStart = new DateOnly(today.Year, today.Month, 1);

        if (query.StartDate is null && query.EndDate is null)
        {
            return (currentMonthStart, EndOfMonth(currentMonthStart));
        }

        if (query.StartDate is not null && query.EndDate is null)
        {
            return (query.StartDate.Value, EndOfMonth(query.StartDate.Value));
        }

        if (query.StartDate is null && query.EndDate is not null)
        {
            var monthStart = new DateOnly(query.EndDate.Value.Year, query.EndDate.Value.Month, 1);
            return (monthStart, query.EndDate.Value);
        }

        return (query.StartDate!.Value, query.EndDate!.Value);
    }

    private static (DateOnly StartMonth, DateOnly EndMonth) ResolveTrendRange(DashboardTrendsQuery query)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var currentMonthStart = new DateOnly(today.Year, today.Month, 1);

        if (query.StartMonth is null && query.EndMonth is null)
        {
            return (currentMonthStart.AddMonths(-5), currentMonthStart);
        }

        if (query.StartMonth is not null && query.EndMonth is null)
        {
            var startMonth = new DateOnly(query.StartMonth.Value.Year, query.StartMonth.Value.Month, 1);
            return (startMonth, startMonth.AddMonths(5));
        }

        if (query.StartMonth is null && query.EndMonth is not null)
        {
            var endMonth = new DateOnly(query.EndMonth.Value.Year, query.EndMonth.Value.Month, 1);
            return (endMonth.AddMonths(-5), endMonth);
        }

        return (
            new DateOnly(query.StartMonth!.Value.Year, query.StartMonth.Value.Month, 1),
            new DateOnly(query.EndMonth!.Value.Year, query.EndMonth.Value.Month, 1));
    }

    private static DateOnly EndOfMonth(DateOnly value)
    {
        return new DateOnly(value.Year, value.Month, DateTime.DaysInMonth(value.Year, value.Month));
    }
}
