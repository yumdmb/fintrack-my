using System.Security.Claims;
using Fintrack.Api.Data;
using Fintrack.Api.Invoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrack.Api.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Accountant},{AppRoles.Staff}")]
public sealed class InvoicesController(InvoiceService invoiceService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<InvoiceSummaryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyCollection<InvoiceSummaryResponse>>> List(CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        return Ok(await invoiceService.ListAsync(companyId, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<InvoiceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InvoiceResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var invoice = await invoiceService.GetAsync(companyId, id, cancellationToken);

        return invoice is null ? NotFound() : Ok(invoice);
    }

    [HttpPost]
    [ProducesResponseType<InvoiceResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<InvoiceResponse>> Create(
        UpsertInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        try
        {
            var invoice = await invoiceService.CreateAsync(companyId, request, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = invoice.Id }, invoice);
        }
        catch (DuplicateInvoiceNumberException exception)
        {
            return Problem(
                title: "Duplicate invoice number",
                detail: exception.Message,
                statusCode: StatusCodes.Status409Conflict);
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<InvoiceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<InvoiceResponse>> Update(
        Guid id,
        UpsertInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        try
        {
            var invoice = await invoiceService.UpdateAsync(companyId, id, request, cancellationToken);
            return invoice is null ? NotFound() : Ok(invoice);
        }
        catch (DuplicateInvoiceNumberException exception)
        {
            return Problem(
                title: "Duplicate invoice number",
                detail: exception.Message,
                statusCode: StatusCodes.Status409Conflict);
        }
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

        return await invoiceService.DeleteAsync(companyId, id, cancellationToken)
            ? NoContent()
            : NotFound();
    }

    private bool TryGetCompanyId(out Guid companyId)
    {
        return Guid.TryParse(User.FindFirstValue("company_id"), out companyId);
    }
}
