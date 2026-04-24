using System.Security.Claims;
using Fintrack.Api.Auth;
using Fintrack.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fintrack.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    UserManager<ApplicationUser> userManager,
    JwtTokenService jwtTokenService) : ControllerBase
{
    [HttpPost("sign-in")]
    [AllowAnonymous]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> SignIn(SignInRequest request)
    {
        var user = await userManager.Users
            .Include(applicationUser => applicationUser.Company)
            .SingleOrDefaultAsync(applicationUser => applicationUser.NormalizedEmail == request.Email.ToUpperInvariant());

        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized(Problem(title: "Invalid credentials", detail: "Invalid email or password."));
        }

        return Ok(await jwtTokenService.CreateTokenAsync(user));
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CurrentUserResponse>> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userId, out var id))
        {
            return Unauthorized();
        }

        var user = await userManager.Users
            .Include(applicationUser => applicationUser.Company)
            .SingleOrDefaultAsync(applicationUser => applicationUser.Id == id);

        if (user is null)
        {
            return Unauthorized();
        }

        var roles = await userManager.GetRolesAsync(user);
        return Ok(new CurrentUserResponse(
            user.Id,
            user.Email ?? string.Empty,
            user.CompanyId,
            user.Company.Name,
            [.. roles]));
    }
}
