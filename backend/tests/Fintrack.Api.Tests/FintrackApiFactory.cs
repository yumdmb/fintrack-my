using Fintrack.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fintrack.Api.Tests;

public sealed class FintrackApiFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail = "admin@fintrack.local";
    public const string AdminPassword = "ChangeMe123!";
    public const string CompanyName = "Fintrack Demo Sdn. Bhd.";
    public const string CompanyRegistrationNumber = "202401000001";
    public const string CompanyTaxIdentificationNumber = "C1234567890";
    public const string CompanySalesAndServiceTaxNumber = "SST12345678";
    public const string CompanyDefaultCurrencyCode = "MYR";

    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(configuration =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = "Host=localhost;Database=unused",
                ["Jwt:Issuer"] = "Fintrack.Api.Tests",
                ["Jwt:Audience"] = "Fintrack.Tests",
                ["Jwt:SigningKey"] = "test-signing-key-with-at-least-thirty-two-characters",
                ["BootstrapAdmin:Email"] = AdminEmail,
                ["BootstrapAdmin:Password"] = AdminPassword,
                ["BootstrapAdmin:CompanyName"] = CompanyName,
                ["BootstrapAdmin:RegistrationNumber"] = CompanyRegistrationNumber,
                ["BootstrapAdmin:TaxIdentificationNumber"] = CompanyTaxIdentificationNumber,
                ["BootstrapAdmin:SalesAndServiceTaxNumber"] = CompanySalesAndServiceTaxNumber,
                ["BootstrapAdmin:DefaultCurrencyCode"] = CompanyDefaultCurrencyCode
            });
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            foreach (var descriptor in services
                         .Where(descriptor => descriptor.ServiceType.FullName?.Contains("IDbContextOptionsConfiguration") == true)
                         .ToList())
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }

    public async Task<SeededTestUser> CreateUserInNewCompanyAsync(
        string email,
        string password,
        string role,
        string? companyName = null)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var company = new Company
        {
            Name = companyName ?? $"Company {Guid.NewGuid():N}"
        };

        dbContext.Companies.Add(company);
        await dbContext.SaveChangesAsync();

        var user = new ApplicationUser
        {
            Email = email,
            UserName = email,
            EmailConfirmed = true,
            CompanyId = company.Id
        };

        ThrowIfFailed(await userManager.CreateAsync(user, password), "Failed to create seeded test user.");
        ThrowIfFailed(await userManager.AddToRoleAsync(user, role), "Failed to assign seeded test user role.");

        return new SeededTestUser(user.Id, email, company.Id, company.Name, [role]);
    }

    public async Task ConfigureCompanyComplianceAsync(
        Guid companyId,
        string registrationNumber = "202401000001",
        string taxIdentificationNumber = "C1234567890",
        string? salesAndServiceTaxNumber = "SST12345678",
        string defaultCurrencyCode = "MYR")
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var company = await dbContext.Companies.SingleAsync(existingCompany => existingCompany.Id == companyId);
        company.RegistrationNumber = registrationNumber;
        company.TaxIdentificationNumber = taxIdentificationNumber;
        company.SalesAndServiceTaxNumber = salesAndServiceTaxNumber;
        company.DefaultCurrencyCode = defaultCurrencyCode;

        await dbContext.SaveChangesAsync();
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

public sealed record SeededTestUser(
    Guid Id,
    string Email,
    Guid CompanyId,
    string CompanyName,
    IReadOnlyCollection<string> Roles);
