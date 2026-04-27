using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Fintrack.Api.Auth;
using Fintrack.Api.Data;
using Fintrack.Api.Expenses;

namespace Fintrack.Api.Tests;

public sealed class ExpenseEndpointTests(FintrackApiFactory factory) : IClassFixture<FintrackApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateListUpdateAndDelete_WithValidExpense_PersistsForCurrentCompany()
    {
        var auth = await SignInAsync(FintrackApiFactory.AdminEmail, FintrackApiFactory.AdminPassword);
        var createRequest = CreateExpenseRequest();

        var created = await SendJsonAsync<ExpenseResponse>(HttpMethod.Post, "/api/expenses", createRequest, auth.AccessToken);

        Assert.Equal(auth.User.CompanyId, created.CompanyId);
        Assert.Equal(createRequest.Category, created.Category);
        Assert.Equal(125.46m, created.Amount);
        Assert.Equal(auth.User.Id, created.CreatedByUserId);

        var listed = await SendJsonAsync<IReadOnlyCollection<ExpenseSummaryResponse>>(
            HttpMethod.Get,
            "/api/expenses",
            null,
            auth.AccessToken);

        Assert.Contains(listed, expense => expense.Id == created.Id);

        var updateRequest = new UpsertExpenseRequest
        {
            ExpenseDate = new DateOnly(2026, 4, 26),
            Category = "Software",
            Description = "Accounting subscription",
            Amount = 89.1m
        };

        var updated = await SendJsonAsync<ExpenseResponse>(
            HttpMethod.Put,
            $"/api/expenses/{created.Id}",
            updateRequest,
            auth.AccessToken);

        Assert.Equal(updateRequest.Category, updated.Category);
        Assert.Equal(updateRequest.Description, updated.Description);
        Assert.Equal(89.1m, updated.Amount);

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/expenses/{created.Id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var deleteResponse = await _client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        using var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/expenses/{created.Id}");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var getResponse = await _client.SendAsync(getRequest);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Get_ForeignCompanyExpense_ReturnsNotFound()
    {
        var foreignEmail = $"expense-foreign-{Guid.NewGuid():N}@fintrack.local";
        var foreignUser = await factory.CreateUserInNewCompanyAsync(foreignEmail, "ExpenseUser123!", AppRoles.Accountant);
        var foreignAuth = await SignInAsync(foreignEmail, "ExpenseUser123!");
        var foreignExpense = await SendJsonAsync<ExpenseResponse>(
            HttpMethod.Post,
            "/api/expenses",
            CreateExpenseRequest(),
            foreignAuth.AccessToken);

        var adminAuth = await SignInAsync(FintrackApiFactory.AdminEmail, FintrackApiFactory.AdminPassword);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/expenses/{foreignExpense.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAuth.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotEqual(adminAuth.User.CompanyId, foreignUser.CompanyId);
    }

    private static UpsertExpenseRequest CreateExpenseRequest()
    {
        return new UpsertExpenseRequest
        {
            ExpenseDate = new DateOnly(2026, 4, 25),
            Category = "Travel",
            Description = "Client meeting transport",
            Amount = 125.46m
        };
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
