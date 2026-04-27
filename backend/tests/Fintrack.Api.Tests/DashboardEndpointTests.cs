using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Fintrack.Api.Auth;
using Fintrack.Api.Dashboard;
using Fintrack.Api.Data;
using Fintrack.Api.Invoices;

namespace Fintrack.Api.Tests;

public sealed class DashboardEndpointTests(FintrackApiFactory factory) : IClassFixture<FintrackApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task SummaryAndTrends_ReturnExpectedAggregatesForReportingRange()
    {
        var auth = await SignInAsAdminAsync();
        await factory.ConfigureCompanyComplianceAsync(auth.User.CompanyId);

        var finalizedApril = await CreateInvoiceAsync(auth.AccessToken, "INV-APR-FINAL", new DateOnly(2026, 4, 10), 100m, 6m);
        await FinalizeInvoiceAsync(finalizedApril.Id, auth.AccessToken);

        await CreateInvoiceAsync(auth.AccessToken, "INV-APR-DRAFT", new DateOnly(2026, 4, 15), 50m, 0m);

        var finalizedMarch = await CreateInvoiceAsync(auth.AccessToken, "INV-MAR-FINAL", new DateOnly(2026, 3, 20), 30m, 0m);
        await FinalizeInvoiceAsync(finalizedMarch.Id, auth.AccessToken);

        await CreateExpenseAsync(auth.AccessToken, new DateOnly(2026, 4, 8), 40m, "Operations");
        await CreateExpenseAsync(auth.AccessToken, new DateOnly(2026, 3, 12), 15m, "Travel");

        var summary = await SendJsonAsync<DashboardSummaryResponse>(
            HttpMethod.Get,
            "/api/dashboard/summary?startDate=2026-04-01&endDate=2026-04-30",
            null,
            auth.AccessToken);

        var trends = await SendJsonAsync<DashboardTrendsResponse>(
            HttpMethod.Get,
            "/api/dashboard/trends?startMonth=2026-03-01&endMonth=2026-04-01",
            null,
            auth.AccessToken);

        Assert.Equal(new DateOnly(2026, 4, 1), summary.StartDate);
        Assert.Equal(new DateOnly(2026, 4, 30), summary.EndDate);
        Assert.Equal(106m, summary.Revenue);
        Assert.Equal(40m, summary.Expenses);
        Assert.Equal(2, summary.InvoiceCount);
        Assert.Equal(106m, summary.OutstandingBalance);

        Assert.Equal(new DateOnly(2026, 3, 1), trends.StartMonth);
        Assert.Equal(new DateOnly(2026, 4, 1), trends.EndMonth);
        Assert.Collection(
            trends.Buckets.OrderBy(bucket => bucket.MonthStart),
            march =>
            {
                Assert.Equal(new DateOnly(2026, 3, 1), march.MonthStart);
                Assert.Equal(30m, march.Revenue);
                Assert.Equal(15m, march.Expenses);
            },
            april =>
            {
                Assert.Equal(new DateOnly(2026, 4, 1), april.MonthStart);
                Assert.Equal(106m, april.Revenue);
                Assert.Equal(40m, april.Expenses);
            });
    }

    [Fact]
    public async Task Summary_WithStaffRole_ReturnsForbidden()
    {
        var staffEmail = $"staff-dashboard-{Guid.NewGuid():N}@fintrack.local";
        await factory.CreateUserInNewCompanyAsync(staffEmail, "StaffUser123!", AppRoles.Staff);
        var auth = await SignInAsync(staffEmail, "StaffUser123!");

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/dashboard/summary");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<InvoiceResponse> CreateInvoiceAsync(
        string accessToken,
        string invoiceNumber,
        DateOnly issueDate,
        decimal unitPrice,
        decimal taxRate)
    {
        var request = new UpsertInvoiceRequest
        {
            InvoiceNumber = invoiceNumber,
            CustomerName = "Dashboard Customer Sdn. Bhd.",
            CustomerRegistrationNumber = "202604240010",
            CustomerTaxIdentificationNumber = "C0000000001",
            IssueDate = issueDate,
            DueDate = issueDate.AddDays(30),
            CurrencyCode = "MYR",
            LineItems =
            [
                new UpsertInvoiceLineItemRequest
                {
                    Description = "Dashboard service",
                    Quantity = 1m,
                    UnitPrice = unitPrice,
                    TaxRate = taxRate
                }
            ]
        };

        return await SendJsonAsync<InvoiceResponse>(HttpMethod.Post, "/api/invoices", request, accessToken);
    }

    private async Task FinalizeInvoiceAsync(Guid invoiceId, string accessToken)
    {
        await SendJsonAsync<InvoiceResponse>(HttpMethod.Post, $"/api/invoices/{invoiceId}/finalize", null, accessToken);
    }

    private async Task CreateExpenseAsync(string accessToken, DateOnly expenseDate, decimal amount, string category)
    {
        var request = new
        {
            ExpenseDate = expenseDate,
            Category = category,
            Description = $"{category} expense",
            Amount = amount
        };

        await SendJsonAsync<object>(HttpMethod.Post, "/api/expenses", request, accessToken);
    }

    private async Task<AuthResponse> SignInAsAdminAsync()
    {
        return await SignInAsync(FintrackApiFactory.AdminEmail, FintrackApiFactory.AdminPassword);
    }

    private async Task<AuthResponse> SignInAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/sign-in", new SignInRequest(email, password));
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AuthResponse>()
            ?? throw new InvalidOperationException("Expected auth response.");
    }

    private async Task<T> SendJsonAsync<T>(HttpMethod method, string requestUri, object? body, string accessToken)
    {
        using var request = new HttpRequestMessage(method, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        if (typeof(T) == typeof(object))
        {
            return (T)(object)new object();
        }

        return await response.Content.ReadFromJsonAsync<T>()
            ?? throw new InvalidOperationException($"Expected {typeof(T).Name} response.");
    }
}
