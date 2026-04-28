using Fintrack.Api.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Fintrack.Api.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (dbContext.Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL")
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                ThrowIfFailed(roleResult, $"Failed to seed role '{role}'.");
            }
        }

        var bootstrapOptions = scope.ServiceProvider.GetRequiredService<IOptions<BootstrapAdminOptions>>().Value;
        if (string.IsNullOrWhiteSpace(bootstrapOptions.Email)
            || string.IsNullOrWhiteSpace(bootstrapOptions.Password)
            || string.IsNullOrWhiteSpace(bootstrapOptions.CompanyName))
        {
            return;
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var existingUser = await userManager.FindByEmailAsync(bootstrapOptions.Email);
        if (existingUser is not null)
        {
            if (!await userManager.IsInRoleAsync(existingUser, AppRoles.Admin))
            {
                ThrowIfFailed(await userManager.AddToRoleAsync(existingUser, AppRoles.Admin), "Failed to assign bootstrap admin role.");
            }

            var existingCompany = await dbContext.Companies.SingleAsync(
                company => company.Id == existingUser.CompanyId,
                cancellationToken);
            ApplyBootstrapCompanyConfiguration(existingCompany, bootstrapOptions);
            await dbContext.SaveChangesAsync(cancellationToken);

            return;
        }

        var company = new Company
        {
            Name = bootstrapOptions.CompanyName
        };
        ApplyBootstrapCompanyConfiguration(company, bootstrapOptions);
        dbContext.Companies.Add(company);
        await dbContext.SaveChangesAsync(cancellationToken);

        var adminUser = new ApplicationUser
        {
            Email = bootstrapOptions.Email,
            UserName = bootstrapOptions.Email,
            CompanyId = company.Id,
            EmailConfirmed = true
        };

        ThrowIfFailed(await userManager.CreateAsync(adminUser, bootstrapOptions.Password), "Failed to create bootstrap admin user.");
        ThrowIfFailed(await userManager.AddToRoleAsync(adminUser, AppRoles.Admin), "Failed to assign bootstrap admin role.");
    }

    private static void ApplyBootstrapCompanyConfiguration(Company company, BootstrapAdminOptions bootstrapOptions)
    {
        company.Name = bootstrapOptions.CompanyName;
        company.RegistrationNumber = NormalizeOptional(bootstrapOptions.RegistrationNumber);
        company.TaxIdentificationNumber = NormalizeOptional(bootstrapOptions.TaxIdentificationNumber);
        company.SalesAndServiceTaxNumber = NormalizeOptional(bootstrapOptions.SalesAndServiceTaxNumber);
        company.DefaultCurrencyCode = NormalizeCurrencyCode(bootstrapOptions.DefaultCurrencyCode);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static string NormalizeCurrencyCode(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "MYR"
            : value.Trim().ToUpperInvariant();
    }

    private static void ThrowIfFailed(IdentityResult result, string message)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"{message} {errors}");
    }
}
