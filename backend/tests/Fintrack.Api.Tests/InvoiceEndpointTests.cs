using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Fintrack.Api.Auth;
using Fintrack.Api.Data;
using Fintrack.Api.Invoices;
using Microsoft.AspNetCore.Mvc;

namespace Fintrack.Api.Tests;

public sealed class InvoiceEndpointTests(FintrackApiFactory factory) : IClassFixture<FintrackApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateAndGet_WithValidInvoice_PersistsLineItemsForCurrentCompany()
    {
        var auth = await SignInAsAdminAsync();
        var createRequest = CreateInvoiceRequest($"INV-{Guid.NewGuid():N}");

        var created = await SendJsonAsync<InvoiceResponse>(HttpMethod.Post, "/api/invoices", createRequest, auth.AccessToken);

        Assert.Equal(createRequest.InvoiceNumber, created.InvoiceNumber);
        Assert.Equal(auth.User.CompanyId, created.CompanyId);
        Assert.Equal(2, created.LineItems.Count);
        Assert.Equal(150m, created.Subtotal);
        Assert.Equal(9m, created.TaxTotal);
        Assert.Equal(159m, created.GrandTotal);

        var fetched = await SendJsonAsync<InvoiceResponse>(HttpMethod.Get, $"/api/invoices/{created.Id}", null, auth.AccessToken);

        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(created.LineItems.Count, fetched.LineItems.Count);
    }

    [Fact]
    public async Task Create_WithRoundedFinanceValues_ComputesServerSideTotals()
    {
        var auth = await SignInAsAdminAsync();
        var createRequest = new UpsertInvoiceRequest
        {
            InvoiceNumber = $"INV-{Guid.NewGuid():N}",
            CustomerName = "Precision Customer Sdn. Bhd.",
            IssueDate = new DateOnly(2026, 4, 24),
            CurrencyCode = "MYR",
            LineItems =
            [
                new UpsertInvoiceLineItemRequest
                {
                    Description = "Rounded quantity and price",
                    Quantity = 1.2345m,
                    UnitPrice = 10.01m,
                    TaxRate = 6.5m
                },
                new UpsertInvoiceLineItemRequest
                {
                    Description = "Second line",
                    Quantity = 2m,
                    UnitPrice = 5.55m,
                    TaxRate = 0m
                }
            ]
        };

        var created = await SendJsonAsync<InvoiceResponse>(HttpMethod.Post, "/api/invoices", createRequest, auth.AccessToken);

        Assert.Equal(23.46m, created.Subtotal);
        Assert.Equal(0.8m, created.TaxTotal);
        Assert.Equal(24.26m, created.GrandTotal);
    }

    [Fact]
    public async Task List_ReturnsOnlyPersistedInvoiceSummaries()
    {
        var auth = await SignInAsAdminAsync();
        var createRequest = CreateInvoiceRequest($"INV-{Guid.NewGuid():N}");
        var created = await SendJsonAsync<InvoiceResponse>(HttpMethod.Post, "/api/invoices", createRequest, auth.AccessToken);

        var summaries = await SendJsonAsync<IReadOnlyCollection<InvoiceSummaryResponse>>(
            HttpMethod.Get,
            "/api/invoices",
            null,
            auth.AccessToken);

        Assert.Contains(summaries, invoice => invoice.Id == created.Id);
    }

    [Fact]
    public async Task Update_ReplacesInvoiceFieldsAndLineItems()
    {
        var auth = await SignInAsAdminAsync();
        var created = await SendJsonAsync<InvoiceResponse>(
            HttpMethod.Post,
            "/api/invoices",
            CreateInvoiceRequest($"INV-{Guid.NewGuid():N}"),
            auth.AccessToken);
        var updateRequest = new UpsertInvoiceRequest
        {
            InvoiceNumber = $"{created.InvoiceNumber}-UPDATED",
            CustomerName = "Updated Customer Sdn. Bhd.",
            IssueDate = new DateOnly(2026, 4, 24),
            DueDate = new DateOnly(2026, 5, 24),
            CurrencyCode = "myr",
            LineItems =
            [
                new UpsertInvoiceLineItemRequest
                {
                    Description = "Replacement service",
                    Quantity = 3m,
                    UnitPrice = 25m,
                    TaxRate = 8m
                }
            ]
        };

        var updated = await SendJsonAsync<InvoiceResponse>(
            HttpMethod.Put,
            $"/api/invoices/{created.Id}",
            updateRequest,
            auth.AccessToken);

        Assert.Equal(updateRequest.InvoiceNumber, updated.InvoiceNumber);
        Assert.Equal("Updated Customer Sdn. Bhd.", updated.CustomerName);
        Assert.Equal("MYR", updated.CurrencyCode);
        Assert.Single(updated.LineItems);
        Assert.Equal(75m, updated.Subtotal);
        Assert.Equal(6m, updated.TaxTotal);
        Assert.Equal(81m, updated.GrandTotal);
    }

    [Fact]
    public async Task Delete_RemovesInvoice()
    {
        var auth = await SignInAsAdminAsync();
        var created = await SendJsonAsync<InvoiceResponse>(
            HttpMethod.Post,
            "/api/invoices",
            CreateInvoiceRequest($"INV-{Guid.NewGuid():N}"),
            auth.AccessToken);

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/invoices/{created.Id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var deleteResponse = await _client.SendAsync(deleteRequest);

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        using var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/invoices/{created.Id}");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var getResponse = await _client.SendAsync(getRequest);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Create_WithInvalidFinancePrecision_ReturnsBadRequest()
    {
        var auth = await SignInAsAdminAsync();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/invoices")
        {
            Content = JsonContent.Create(new UpsertInvoiceRequest
            {
                InvoiceNumber = $"INV-{Guid.NewGuid():N}",
                CustomerName = "Invalid Precision Customer",
                IssueDate = new DateOnly(2026, 4, 24),
                CurrencyCode = "MYR",
                LineItems =
                [
                    new UpsertInvoiceLineItemRequest
                    {
                        Description = "Too many decimal places",
                        Quantity = 1.23456m,
                        UnitPrice = 10m,
                        TaxRate = 6m
                    }
                ]
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("LineItems[0].Quantity", problem.Errors.Keys);
    }

    [Fact]
    public async Task Get_ForeignCompanyInvoice_ReturnsNotFound()
    {
        var foreignEmail = $"invoice-foreign-{Guid.NewGuid():N}@fintrack.local";
        var foreignUser = await factory.CreateUserInNewCompanyAsync(foreignEmail, "InvoiceUser123!", AppRoles.Accountant);
        var foreignAuth = await SignInAsync(foreignEmail, "InvoiceUser123!");
        var foreignInvoice = await SendJsonAsync<InvoiceResponse>(
            HttpMethod.Post,
            "/api/invoices",
            CreateInvoiceRequest($"INV-{Guid.NewGuid():N}"),
            foreignAuth.AccessToken);

        var adminAuth = await SignInAsAdminAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/invoices/{foreignInvoice.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAuth.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotEqual(adminAuth.User.CompanyId, foreignUser.CompanyId);
    }

    private static UpsertInvoiceRequest CreateInvoiceRequest(string invoiceNumber)
    {
        return new UpsertInvoiceRequest
        {
            InvoiceNumber = invoiceNumber,
            CustomerName = "Customer Sdn. Bhd.",
            CustomerRegistrationNumber = "202604240001",
            CustomerTaxIdentificationNumber = "C1234567890",
            IssueDate = new DateOnly(2026, 4, 24),
            DueDate = new DateOnly(2026, 5, 24),
            CurrencyCode = "MYR",
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
