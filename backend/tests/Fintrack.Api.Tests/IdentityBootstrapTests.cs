using Fintrack.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fintrack.Api.Tests;

public sealed class IdentityBootstrapTests(FintrackApiFactory factory) : IClassFixture<FintrackApiFactory>
{
    [Fact]
    public async Task Startup_SeedsRolesCompanyAndBootstrapAdmin()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in AppRoles.All)
        {
            Assert.True(await roleManager.RoleExistsAsync(role));
        }

        var admin = await dbContext.Users
            .Include(user => user.Company)
            .SingleOrDefaultAsync(user => user.NormalizedEmail == FintrackApiFactory.AdminEmail.ToUpperInvariant());
        Assert.NotNull(admin);
        Assert.True(await userManager.IsInRoleAsync(admin, AppRoles.Admin));
        Assert.Equal(FintrackApiFactory.CompanyName, admin.Company.Name);
    }
}
