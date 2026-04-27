using Fintrack.Api.Auth;
using Fintrack.Api.Dashboard;
using Fintrack.Api.Configuration;
using Fintrack.Api.Data;
using Fintrack.Api.Expenses;
using Fintrack.Api.Invoices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Fintrack.Api;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddUserSecrets<Program>(optional: true);

        builder.Services.AddControllers();
        builder.Services.AddHealthChecks();
        builder.Services.AddOpenApi();
        builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            };
        });
        builder.Services.AddOptions<CorsOptions>()
            .Bind(builder.Configuration.GetSection(CorsOptions.SectionName));
        builder.Services.AddOptions<JwtOptions>()
            .Bind(builder.Configuration.GetSection(JwtOptions.SectionName));
        builder.Services.AddOptions<BootstrapAdminOptions>()
            .Bind(builder.Configuration.GetSection(BootstrapAdminOptions.SectionName));
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(CorsOptions.PolicyName, policy =>
            {
                var corsOptions = builder.Configuration
                    .GetSection(CorsOptions.SectionName)
                    .Get<CorsOptions>() ?? new CorsOptions();

                if (corsOptions.AllowedOrigins.Count == 0)
                {
                    return;
                }

                policy.WithOrigins([.. corsOptions.AllowedOrigins])
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
        builder.Services.AddDbContext<ApplicationDbContext>((services, options) =>
        {
            var configuration = services.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("Postgres");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:Postgres is not configured.");
            }

            options.UseNpgsql(connectionString);
        });
        builder.Services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();
        builder.Services.AddScoped<JwtTokenService>();
        builder.Services.AddScoped<InvoiceService>();
        builder.Services.AddScoped<InvoiceExportService>();
        builder.Services.AddScoped<ExpenseService>();
        builder.Services.AddScoped<DashboardService>();
        builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>, JwtBearerOptionsSetup>();
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();
        builder.Services.AddAuthorization();

        var app = builder.Build();

        app.UseExceptionHandler();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseCors(CorsOptions.PolicyName);
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHealthChecks("/health");
        app.MapGet("/", () => Results.Redirect("/openapi/v1.json", permanent: false));
        app.MapControllers();

        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
        {
            await IdentitySeeder.SeedAsync(app.Services);
        }

        await app.RunAsync();
    }
}
