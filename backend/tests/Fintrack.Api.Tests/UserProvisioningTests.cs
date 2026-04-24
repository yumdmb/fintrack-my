using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Fintrack.Api.Auth;
using Fintrack.Api.Data;

namespace Fintrack.Api.Tests;

public sealed class UserProvisioningTests(FintrackApiFactory factory) : IClassFixture<FintrackApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Admin_CanProvisionAccountantInOwnCompany_AndNewUserCanSignIn()
    {
        var email = $"accountant-{Guid.NewGuid():N}@fintrack.local";
        var created = await CreateUserAsync(email, "Accountant123!", AppRoles.Accountant);

        Assert.Equal(email, created.Email);
        Assert.Contains(AppRoles.Accountant, created.Roles);

        var signInResponse = await _client.PostAsJsonAsync("/api/auth/sign-in", new SignInRequest(email, "Accountant123!"));
        Assert.Equal(HttpStatusCode.OK, signInResponse.StatusCode);

        var auth = await signInResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.Contains(AppRoles.Accountant, auth.User.Roles);
        Assert.Equal(created.CompanyId, auth.User.CompanyId);
    }

    [Fact]
    public async Task Staff_CannotProvisionUsers()
    {
        var staffEmail = $"staff-{Guid.NewGuid():N}@fintrack.local";
        await CreateUserAsync(staffEmail, "StaffUser123!", AppRoles.Staff);
        var staffAuth = await SignInAsync(staffEmail, "StaffUser123!");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/users")
        {
            Content = JsonContent.Create(new CreateUserRequest(
                $"blocked-{Guid.NewGuid():N}@fintrack.local",
                "Blocked123!",
                AppRoles.Accountant))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", staffAuth.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<UserResponse> CreateUserAsync(string email, string password, string role)
    {
        var adminAuth = await SignInAsync(FintrackApiFactory.AdminEmail, FintrackApiFactory.AdminPassword);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/users")
        {
            Content = JsonContent.Create(new CreateUserRequest(email, password, role))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAuth.AccessToken);

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<UserResponse>()
            ?? throw new InvalidOperationException("Expected user response.");
    }

    private async Task<AuthResponse> SignInAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/sign-in", new SignInRequest(email, password));
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AuthResponse>()
            ?? throw new InvalidOperationException("Expected auth response.");
    }
}
