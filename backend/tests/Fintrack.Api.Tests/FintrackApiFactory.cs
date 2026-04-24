using Fintrack.Api.Data;
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
                ["BootstrapAdmin:CompanyName"] = CompanyName
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
}
