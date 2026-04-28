using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Fintrack.Api.Auth;
using Fintrack.Api.Invoices;
using Microsoft.AspNetCore.Mvc;

namespace Fintrack.Api.Tests;

public sealed class InvoiceExportEndpointTests(FintrackApiFactory factory) : IClassFixture<FintrackApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Create_WithoutCurrencyCode_UsesCompanyDefaultCurrency()
    {
        var auth = await SignInAsAdminAsync();
        var created = await SendJsonAsync<InvoiceResponse>(
            HttpMethod.Post,
            "/api/invoices",
            CreateInvoiceRequest($"INV-{Guid.NewGuid():N}", currencyCode: null),
            auth.AccessToken);

        Assert.Equal("MYR", created.CurrencyCode);
        Assert.Equal("Draft", created.Status);
    }

    [Fact]
    public async Task ExportJson_AfterFinalization_ReturnsMalaysiaExportShape()
    {
        var auth = await SignInAsAdminAsync();
        await factory.ConfigureCompanyComplianceAsync(auth.User.CompanyId);

        var created = await SendJsonAsync<InvoiceResponse>(
            HttpMethod.Post,
            "/api/invoices",
            CreateInvoiceRequest($"INV-{Guid.NewGuid():N}", currencyCode: null),
            auth.AccessToken);

        var finalized = await SendJsonAsync<InvoiceResponse>(
            HttpMethod.Post,
            $"/api/invoices/{created.Id}/finalize",
            null,
            auth.AccessToken);

        var export = await SendJsonAsync<InvoiceJsonExportResponse>(
            HttpMethod.Get,
            $"/api/invoices/{created.Id}/export/json",
            null,
            auth.AccessToken);

        Assert.Equal("Finalized", finalized.Status);
        Assert.Equal(created.Id, export.InvoiceId);
        Assert.Equal(created.InvoiceNumber, export.InvoiceNumber);
        Assert.Equal("MYR", export.CurrencyCode);
        Assert.Equal(FintrackApiFactory.CompanyName, export.Seller.Name);
        Assert.Equal("202401000001", export.Seller.RegistrationNumber);
        Assert.Equal("C1234567890", export.Seller.TaxIdentificationNumber);
        Assert.Equal("SST12345678", export.Seller.SalesAndServiceTaxNumber);
        Assert.Equal("Customer Sdn. Bhd.", export.Buyer.Name);
        Assert.Equal(2, export.LineItems.Count);
        Assert.Equal(159m, export.GrandTotal);
    }

    [Fact]
    public async Task ExportCsv_ForDraftInvoice_ReturnsValidationFailure()
    {
        var auth = await SignInAsAdminAsync();
        await factory.ConfigureCompanyComplianceAsync(auth.User.CompanyId);

        var created = await SendJsonAsync<InvoiceResponse>(
            HttpMethod.Post,
            "/api/invoices",
            CreateInvoiceRequest($"INV-{Guid.NewGuid():N}"),
            auth.AccessToken);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/invoices/{created.Id}/export/csv");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("status", problem.Errors.Keys);
    }

    [Fact]
    public async Task Finalize_WithMissingComplianceFields_ReturnsValidationFailure()
    {
        var auth = await SignInAsAdminAsync();
        await factory.ConfigureCompanyComplianceAsync(
            auth.User.CompanyId,
            registrationNumber: "",
            taxIdentificationNumber: "",
            salesAndServiceTaxNumber: null);
        var created = await SendJsonAsync<InvoiceResponse>(
            HttpMethod.Post,
            "/api/invoices",
            CreateInvoiceRequest($"INV-{Guid.NewGuid():N}"),
            auth.AccessToken);

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/invoices/{created.Id}/finalize");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("seller.registrationNumber", problem.Errors.Keys);
        Assert.Contains("seller.taxIdentificationNumber", problem.Errors.Keys);
    }

    [Fact]
    public async Task ExportCsv_AfterFinalization_ReturnsCsvRows()
    {
        var auth = await SignInAsAdminAsync();
        await factory.ConfigureCompanyComplianceAsync(auth.User.CompanyId);

        var created = await SendJsonAsync<InvoiceResponse>(
            HttpMethod.Post,
            "/api/invoices",
            CreateInvoiceRequest($"INV-{Guid.NewGuid():N}"),
            auth.AccessToken);

        await SendJsonAsync<InvoiceResponse>(
            HttpMethod.Post,
            $"/api/invoices/{created.Id}/finalize",
            null,
            auth.AccessToken);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/invoices/{created.Id}/export/csv");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        var csv = await response.Content.ReadAsStringAsync();
        Assert.Contains("InvoiceNumber,Status,IssueDate", csv);
        Assert.Contains(created.InvoiceNumber, csv);
        Assert.Contains("Consulting", csv);
        Assert.Contains("Support", csv);
    }

    private static UpsertInvoiceRequest CreateInvoiceRequest(string invoiceNumber, string? currencyCode = "MYR")
    {
        return new UpsertInvoiceRequest
        {
            InvoiceNumber = invoiceNumber,
            CustomerName = "Customer Sdn. Bhd.",
            CustomerRegistrationNumber = "202604240001",
            CustomerTaxIdentificationNumber = "C1234567890",
            IssueDate = new DateOnly(2026, 4, 24),
            DueDate = new DateOnly(2026, 5, 24),
            CurrencyCode = currencyCode,
            LineItems =
            [
                new UpsertInvoiceLineItemRequest
                {
                    Description = "Consulting",
                    Quantity = 2m,
                    UnitPrice = 50m,
                    TaxRate = 6m
                },
                new UpsertInvoiceLineItemRequest
                {
                    Description = "Support",
                    Quantity = 1m,
                    UnitPrice = 50m,
                    TaxRate = 6m
                }
            ]
        };
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

        return await response.Content.ReadFromJsonAsync<T>()
            ?? throw new InvalidOperationException($"Expected {typeof(T).Name} response.");
    }
}
