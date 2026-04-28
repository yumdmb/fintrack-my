using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Fintrack.Api.Auth;
using Fintrack.Api.Dashboard;
using Fintrack.Api.Expenses;
using Fintrack.Api.Invoices;

namespace Fintrack.Api.Tests;

public sealed class SmokeWorkflowTests(FintrackApiFactory factory) : IClassFixture<FintrackApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task SignInInvoiceExpenseDashboardAndExport_WorkAsOneFlow()
    {
        var auth = await SignInAsync(FintrackApiFactory.AdminEmail, FintrackApiFactory.AdminPassword);

        var invoice = await SendJsonAsync<InvoiceResponse>(
            HttpMethod.Post,
            "/api/invoices",
            new UpsertInvoiceRequest
            {
                InvoiceNumber = $"SMOKE-{Guid.NewGuid():N}",
                CustomerName = "Smoke Customer Sdn. Bhd.",
                CustomerRegistrationNumber = "202604290001",
                CustomerTaxIdentificationNumber = "C0000000001",
                IssueDate = new DateOnly(2026, 4, 29),
                DueDate = new DateOnly(2026, 5, 29),
                LineItems =
                [
                    new UpsertInvoiceLineItemRequest
                    {
                        Description = "Smoke consulting",
                        Quantity = 2m,
                        UnitPrice = 75m,
                        TaxRate = 6m
                    }
                ]
            },
            auth.AccessToken);

        var expense = await SendJsonAsync<ExpenseResponse>(
            HttpMethod.Post,
            "/api/expenses",
            new UpsertExpenseRequest
            {
                ExpenseDate = new DateOnly(2026, 4, 29),
                Category = "Smoke Ops",
                Description = "Smoke verification expense",
                Amount = 42.5m
            },
            auth.AccessToken);

        var finalized = await SendJsonAsync<InvoiceResponse>(
            HttpMethod.Post,
            $"/api/invoices/{invoice.Id}/finalize",
            null,
            auth.AccessToken);

        var dashboard = await SendJsonAsync<DashboardSummaryResponse>(
            HttpMethod.Get,
            "/api/dashboard/summary?startDate=2026-04-01&endDate=2026-04-30",
            null,
            auth.AccessToken);

        var exportJson = await SendJsonAsync<InvoiceJsonExportResponse>(
            HttpMethod.Get,
            $"/api/invoices/{invoice.Id}/export/json",
            null,
            auth.AccessToken);

        using var exportCsvRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/invoices/{invoice.Id}/export/csv");
        exportCsvRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var exportCsvResponse = await _client.SendAsync(exportCsvRequest);

        Assert.Equal(FintrackApiFactory.AdminEmail, auth.User.Email);
        Assert.Equal(auth.User.CompanyId, invoice.CompanyId);
        Assert.Equal(auth.User.CompanyId, expense.CompanyId);
        Assert.Equal("Finalized", finalized.Status);
        Assert.True(dashboard.Revenue >= finalized.GrandTotal);
        Assert.True(dashboard.Expenses >= expense.Amount);
        Assert.Equal(invoice.InvoiceNumber, exportJson.InvoiceNumber);
        Assert.Equal(FintrackApiFactory.CompanyRegistrationNumber, exportJson.Seller.RegistrationNumber);
        Assert.Equal(FintrackApiFactory.CompanyTaxIdentificationNumber, exportJson.Seller.TaxIdentificationNumber);
        Assert.Equal(HttpStatusCode.OK, exportCsvResponse.StatusCode);
        Assert.Equal("text/csv; charset=utf-8", exportCsvResponse.Content.Headers.ContentType?.ToString());

        var csv = await exportCsvResponse.Content.ReadAsStringAsync();
        Assert.Contains(invoice.InvoiceNumber, csv);
        Assert.Contains("Smoke consulting", csv);
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

        return await response.Content.ReadFromJsonAsync<T>()
            ?? throw new InvalidOperationException($"Expected {typeof(T).Name} response.");
    }
}
