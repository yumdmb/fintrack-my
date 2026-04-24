using System.Security.Claims;
using Fintrack.Api.Auth;
using Fintrack.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fintrack.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController(
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserResponse>> CreateUser(CreateUserRequest request)
    {
        var normalizedRole = request.Role.Trim().ToLowerInvariant();
        if (!AppRoles.All.Contains(normalizedRole, StringComparer.Ordinal))
        {
            ModelState.AddModelError(nameof(request.Role), $"Role must be one of: {string.Join(", ", AppRoles.All)}.");
            return ValidationProblem(ModelState);
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(currentUserId, out var id))
        {
            return Unauthorized();
        }

        var currentUser = await userManager.Users.SingleOrDefaultAsync(user => user.Id == id);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
            EmailConfirmed = true,
            CompanyId = currentUser.CompanyId
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            AddIdentityErrors(createResult);
            return ValidationProblem(ModelState);
        }

        var roleResult = await userManager.AddToRoleAsync(user, normalizedRole);
        if (!roleResult.Succeeded)
        {
            AddIdentityErrors(roleResult);
            return ValidationProblem(ModelState);
        }

        return Created($"/api/users/{user.Id}", new UserResponse(
            user.Id,
            user.Email ?? string.Empty,
            user.CompanyId,
            [normalizedRole]));
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.Code, error.Description);
        }
    }
}
