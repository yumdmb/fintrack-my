using Fintrack.Api.Data;
using Fintrack.Api.Finance;
using Microsoft.EntityFrameworkCore;

namespace Fintrack.Api.Expenses;

public sealed class ExpenseService(ApplicationDbContext dbContext)
{
    public async Task<IReadOnlyCollection<ExpenseSummaryResponse>> ListAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Expenses
            .AsNoTracking()
            .Where(expense => expense.CompanyId == companyId)
            .OrderByDescending(expense => expense.ExpenseDate)
            .ThenByDescending(expense => expense.Id)
            .Select(expense => new ExpenseSummaryResponse(
                expense.Id,
                expense.ExpenseDate,
                expense.Category,
                expense.Description,
                expense.Amount,
                expense.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<ExpenseResponse?> GetAsync(
        Guid companyId,
        Guid expenseId,
        CancellationToken cancellationToken = default)
    {
        var expense = await FindExpenseAsync(companyId, expenseId, tracking: false, cancellationToken);
        return expense is null ? null : Map(expense);
    }

    public async Task<ExpenseResponse> CreateAsync(
        Guid companyId,
        Guid createdByUserId,
        UpsertExpenseRequest request,
        CancellationToken cancellationToken = default)
    {
        var expense = new Expense
        {
            CompanyId = companyId,
            CreatedByUserId = createdByUserId
        };

        Apply(expense, request);

        dbContext.Expenses.Add(expense);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(expense);
    }

    public async Task<ExpenseResponse?> UpdateAsync(
        Guid companyId,
        Guid expenseId,
        UpsertExpenseRequest request,
        CancellationToken cancellationToken = default)
    {
        var expense = await FindExpenseAsync(companyId, expenseId, tracking: true, cancellationToken);
        if (expense is null)
        {
            return null;
        }

        Apply(expense, request);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(expense);
    }

    public async Task<bool> DeleteAsync(
        Guid companyId,
        Guid expenseId,
        CancellationToken cancellationToken = default)
    {
        var expense = await FindExpenseAsync(companyId, expenseId, tracking: true, cancellationToken);
        if (expense is null)
        {
            return false;
        }

        dbContext.Expenses.Remove(expense);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<Expense?> FindExpenseAsync(
        Guid companyId,
        Guid expenseId,
        bool tracking,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Expenses
            .Where(expense => expense.CompanyId == companyId && expense.Id == expenseId);

        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(cancellationToken);
    }

    private static void Apply(Expense expense, UpsertExpenseRequest request)
    {
        expense.ExpenseDate = request.ExpenseDate;
        expense.Category = request.Category.Trim();
        expense.Description = request.Description.Trim();
        expense.Amount = FinanceValueRules.RoundCurrency(request.Amount);
    }

    private static ExpenseResponse Map(Expense expense)
    {
        return new ExpenseResponse(
            expense.Id,
            expense.CompanyId,
            expense.ExpenseDate,
            expense.Category,
            expense.Description,
            expense.Amount,
            expense.CreatedByUserId,
            expense.CreatedAt);
    }
}
