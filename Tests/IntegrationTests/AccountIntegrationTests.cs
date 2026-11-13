
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Infra.Entities;
using Infra.Entities.HolidayTracking.Accounts;
using Infra.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Service;
using Service.DTOs.LoginDTOs;
using Service.DTOs.UserDTOs;
using Service.Security;

namespace Tests.IntegrationTests;

public class AccountIntegrationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AccountIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // --- HELPER METHODS ---

    #region Helper methods

    private async Task<string> LoginAsAdmin()
    {
        const string username = "integration_admin";
        const string password = "AdminPassword123";

        using var scope = _factory.Services.CreateScope();

        var repo = scope.ServiceProvider.GetRequiredService<IUserRepo>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var admin = await repo.GetByUsername(username);

        if (admin == null)
        {
            admin = new User
            {
                UserId = Guid.NewGuid().ToString(),
                Username = username,
                PasswordHash = hasher.HashAndSaltPassword(password),
                IsAdmin = true,
                IsActive = true
            };

            await repo.Insert(admin);
        }

        var response = await _client.PostAsJsonAsync(
            "/login",
            new
            {
                Username = username,
                Password = password
            });

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();

        return result!.Token;
    }

    private async Task AuthorizeAsAdmin()
    {
        var token = await LoginAsAdmin();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<string> GetAdminUserId()
    {
        using var scope = _factory.Services.CreateScope();

        var repo = scope.ServiceProvider.GetRequiredService<IUserRepo>();
        var admin = await repo.GetByUsername("integration_admin");

        return admin!.UserId;
    }

    private async Task<(string UserId, List<HolidayAccount> accounts)> CreateUserWithAccounts()
    {
        using var scope = _factory.Services.CreateScope();

        var userService = scope.ServiceProvider.GetRequiredService<UserService>();

        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepo>();

        var accountRepo = scope.ServiceProvider.GetRequiredService<IAccountRepo>();

        var username = $"account_test_{Guid.NewGuid():N}";

        await userService.Insert(new CreateUserDto
        {
            Username = username,
            Password = "TestPassword123",
            IsAdmin = false
        });

        var user = await userRepo.GetByUsername(username);

        var accounts = await accountRepo.GetUserAccountsAsync(user!.UserId);

        return (user.UserId, accounts.ToList());
    }

    #endregion

    // --- READ TESTS ---

    #region Read Tests

    [Fact]
    public async Task GetAccountById_WithExistingId_ReturnsAccount()
    {
        // Arrange
        await AuthorizeAsAdmin();

        var (userId, accounts) = await CreateUserWithAccounts();

        var createdAccount = accounts.Single(
            a => a.Type == HolidayType.Week1_5);

        // Act
        var response = await _client.GetAsync(
            $"/GetAccountById?id={createdAccount.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var account = await response.Content.ReadFromJsonAsync<HolidayAccount>();

        Assert.NotNull(account);
        Assert.Equal(createdAccount.Id, account.Id);
        Assert.Equal(userId, account.UserId);
    }

    [Fact]
    public async Task GetAccountById_WithUnknownId_ReturnsNotFound()
    {
        // Arrange
        await AuthorizeAsAdmin();

        // Act
        var response = await _client.GetAsync(
            $"/GetAccountById?id={Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAccountsForUser_WithExistingAccounts_ReturnsUserAccounts()
    {
        // Arrange
        await AuthorizeAsAdmin();

        var (userId, _) = await CreateUserWithAccounts();

        // Act
        var response = await _client.GetAsync(
            $"/GetAccountsForUser?userId={Uri.EscapeDataString(userId)}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var accounts =
            await response.Content.ReadFromJsonAsync<List<HolidayAccount>>();

        Assert.NotNull(accounts);
        Assert.Equal(2, accounts.Count);

        Assert.Contains(accounts, a => a.Type == HolidayType.Week1_5);
        Assert.Contains(accounts, a => a.Type == HolidayType.Week6);

        Assert.All(accounts, account =>
            Assert.Equal(userId, account.UserId));
    }

    [Fact]
    public async Task GetAccountsForUser_WithUnknownUser_ReturnsEmptyList()
    {
        // Arrange
        await AuthorizeAsAdmin();

        var unknownUserId = Guid.NewGuid().ToString();

        // Act
        var response = await _client.GetAsync(
            $"/GetAccountsForUser?userId={unknownUserId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var accounts =
            await response.Content.ReadFromJsonAsync<List<HolidayAccount>>();

        Assert.NotNull(accounts);
        Assert.Empty(accounts);
    }

    #endregion
}