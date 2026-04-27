using System.Security.Claims;
using Fintrack.Api.Data;
using Fintrack.Api.Expenses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrack.Api.Controllers;

[ApiController]
[Route("api/expenses")]
[Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Accountant},{AppRoles.Staff}")]
public sealed class ExpensesController(ExpenseService expenseService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<ExpenseSummaryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyCollection<ExpenseSummaryResponse>>> List(CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        return Ok(await expenseService.ListAsync(companyId, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ExpenseResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExpenseResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var expense = await expenseService.GetAsync(companyId, id, cancellationToken);
        return expense is null ? NotFound() : Ok(expense);
    }

    [HttpPost]
    [ProducesResponseType<ExpenseResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ExpenseResponse>> Create(
        UpsertExpenseRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out var companyId, out var userId))
        {
            return Unauthorized();
        }

        var expense = await expenseService.CreateAsync(companyId, userId, request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = expense.Id }, expense);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<ExpenseResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExpenseResponse>> Update(
        Guid id,
        UpsertExpenseRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var expense = await expenseService.UpdateAsync(companyId, id, request, cancellationToken);
        return expense is null ? NotFound() : Ok(expense);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        return await expenseService.DeleteAsync(companyId, id, cancellationToken)
            ? NoContent()
            : NotFound();
    }

    private bool TryGetCompanyId(out Guid companyId)
    {
        return Guid.TryParse(User.FindFirstValue("company_id"), out companyId);
    }

    private bool TryGetCurrentUser(out Guid companyId, out Guid userId)
    {
        companyId = Guid.Empty;
        userId = Guid.Empty;

        return Guid.TryParse(User.FindFirstValue("company_id"), out companyId)
            && Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
    }
}
