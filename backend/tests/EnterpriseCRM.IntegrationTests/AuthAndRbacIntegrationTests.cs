using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.Auth;
using EnterpriseCRM.Application.Features.Organization;
using EnterpriseCRM.Domain.Constants;
using EnterpriseCRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace EnterpriseCRM.IntegrationTests;

[Collection("IntegrationTests")]
public class AuthAndRbacIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AuthAndRbacIntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _output = output;
    }

    [Fact]
    public async Task Step1_VerifyDatabaseTables_ExistInSupabase()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var tableNames = await context.Database
            .SqlQueryRaw<string>("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' ORDER BY table_name;")
            .ToListAsync();

        _output.WriteLine($"Supabase public tables count: {tableNames.Count}");
        foreach (var name in tableNames)
        {
            _output.WriteLine($"  - {name}");
        }

        // Verify key tables exist
        Assert.Contains("users", tableNames);
        Assert.Contains("roles", tableNames);
        Assert.Contains("permissions", tableNames);
        Assert.Contains("user_roles", tableNames);
        Assert.Contains("role_permissions", tableNames);
        Assert.Contains("departments", tableNames);
        Assert.Contains("teams", tableNames);
        Assert.Contains("employees", tableNames);
        Assert.Contains("leads", tableNames);
        Assert.Contains("deals", tableNames);
        Assert.Contains("contracts", tableNames);
        Assert.Contains("invoices", tableNames);
        Assert.Contains("payments", tableNames);
        Assert.Contains("__EFMigrationsHistory", tableNames);
    }

    [Fact]
    public async Task Step2_Login_WithValidAdminCredentials_ShouldSucceed()
    {
        // Arrange
        var request = new LoginRequest("admin@enterprisecrm.local", "EnterpriseCRM@2026!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);
        var content = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Login response: {content}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(content, JsonOptions);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.False(string.IsNullOrWhiteSpace(result.Data.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.Data.RefreshToken));
        Assert.Contains(SystemRoles.Admin, result.Data.User.Roles);
    }

    [Fact]
    public async Task Step3_Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest("admin@enterprisecrm.local", "WrongPassword@999!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(content, JsonOptions);
        Assert.NotNull(result);
        Assert.False(result.Success);
    }

    [Fact]
    public async Task Step4_GetCurrentUser_WithBearerToken_ShouldReturnProfile()
    {
        // 1. Login as Admin
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest("admin@enterprisecrm.local", "EnterpriseCRM@2026!"));
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>(JsonOptions);
        var token = loginResult!.Data!.AccessToken;

        // 2. Call /api/v1/auth/me with Bearer token
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>(JsonOptions);
        Assert.NotNull(result?.Data);
        Assert.Equal("admin@enterprisecrm.local", result.Data.Email);
        Assert.Contains(SystemRoles.Admin, result.Data.Roles);
    }

    [Fact]
    public async Task Step5_GetCurrentUser_WithoutToken_ShouldReturnUnauthorized()
    {
        // Act - Request without Authorization header
        var response = await _client.GetAsync("/api/v1/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Step6_RefreshTokenRotation_ShouldReturnNewTokens()
    {
        // 1. Login to get initial tokens
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest("admin@enterprisecrm.local", "EnterpriseCRM@2026!"));
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>(JsonOptions);
        var oldRefreshToken = loginResult!.Data!.RefreshToken;

        // 2. Refresh token
        var refreshResponse = await _client.PostAsJsonAsync("/api/v1/auth/refresh-token",
            new RefreshTokenRequest(oldRefreshToken));

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        var refreshResult = await refreshResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>(JsonOptions);
        Assert.NotNull(refreshResult?.Data);
        Assert.False(string.IsNullOrWhiteSpace(refreshResult.Data.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(refreshResult.Data.RefreshToken));
        // Token Rotation: new refresh token must differ from old one
        Assert.NotEqual(oldRefreshToken, refreshResult.Data.RefreshToken);
    }

    [Fact]
    public async Task Step7_RbacEnforcement_SalesRoleCannotCreateDepartment()
    {
        // 1. Login as Sales user
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest("sales@enterprisecrm.local", "EnterpriseCRM@2026!"));
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>(JsonOptions);
        var salesToken = loginResult!.Data!.AccessToken;

        // 2. Sales attempts to create a Department (requires departments:manage permission)
        var createDeptRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/departments")
        {
            Content = JsonContent.Create(new CreateDepartmentRequest("DEP-UNAUTH", "Unauthorized Dept", null, null))
        };
        createDeptRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);

        var response = await _client.SendAsync(createDeptRequest);

        // Assert: Sales must be Forbidden (403)
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Step8_RbacEnforcement_AdminRoleCanCreateAndReadDepartment()
    {
        // 1. Login as Admin user
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest("admin@enterprisecrm.local", "EnterpriseCRM@2026!"));
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>(JsonOptions);
        var adminToken = loginResult!.Data!.AccessToken;

        // 2. Admin creates a unique Department
        var uniqueCode = $"TEST-{Guid.NewGuid().ToString("N")[..4].ToUpper()}";
        var createDeptRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/departments")
        {
            Content = JsonContent.Create(new CreateDepartmentRequest(uniqueCode, "Integration Test Dept", "Test Description", null))
        };
        createDeptRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _client.SendAsync(createDeptRequest);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createResult = await response.Content.ReadFromJsonAsync<ApiResponse<DepartmentDto>>(JsonOptions);
        Assert.NotNull(createResult?.Data);
        Assert.Equal(uniqueCode, createResult.Data.Code);

        // 3. Clean up by deleting the test department
        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/departments/{createResult.Data.Id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var deleteResponse = await _client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
    }
}
