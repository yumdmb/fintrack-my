using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Fintrack.Api.Auth;
using Fintrack.Api.Data;

namespace Fintrack.Api.Tests;

public sealed class AuthEndpointTests(FintrackApiFactory factory) : IClassFixture<FintrackApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task SignIn_WithValidAdminCredentials_ReturnsAccessTokenAndIdentity()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/sign-in", new SignInRequest(
            FintrackApiFactory.AdminEmail,
            FintrackApiFactory.AdminPassword));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));
        Assert.Equal(FintrackApiFactory.AdminEmail, body.User.Email);
        Assert.Equal(FintrackApiFactory.CompanyName, body.User.CompanyName);
        Assert.Contains(AppRoles.Admin, body.User.Roles);
    }

    [Fact]
    public async Task SignIn_WithInvalidCredentials_ReturnsUnauthorizedWithoutToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/sign-in", new SignInRequest(
            FintrackApiFactory.AdminEmail,
            "WrongPassword123!"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithValidToken_ReturnsCurrentUserCompanyAndRoles()
    {
        var auth = await SignInAsync(FintrackApiFactory.AdminEmail, FintrackApiFactory.AdminPassword);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CurrentUserResponse>();
        Assert.NotNull(body);
        Assert.Equal(FintrackApiFactory.AdminEmail, body.Email);
        Assert.Equal(FintrackApiFactory.CompanyName, body.CompanyName);
        Assert.Contains(AppRoles.Admin, body.Roles);
    }

    private async Task<AuthResponse> SignInAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/sign-in", new SignInRequest(email, password));
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AuthResponse>()
            ?? throw new InvalidOperationException("Expected auth response.");
    }
}
