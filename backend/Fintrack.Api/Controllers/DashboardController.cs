using System.Security.Claims;
using Fintrack.Api.Dashboard;
using Fintrack.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrack.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Accountant}")]
public sealed class DashboardController(DashboardService dashboardService) : ControllerBase
{
    [HttpGet("summary")]
    [ProducesResponseType<DashboardSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DashboardSummaryResponse>> Summary(
        [FromQuery] DashboardSummaryQuery query,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        return Ok(await dashboardService.GetSummaryAsync(companyId, query, cancellationToken));
    }

    [HttpGet("trends")]
    [ProducesResponseType<DashboardTrendsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DashboardTrendsResponse>> Trends(
        [FromQuery] DashboardTrendsQuery query,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        return Ok(await dashboardService.GetTrendsAsync(companyId, query, cancellationToken));
    }

    private bool TryGetCompanyId(out Guid companyId)
    {
        return Guid.TryParse(User.FindFirstValue("company_id"), out companyId);
    }
}
