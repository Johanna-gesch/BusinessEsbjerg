using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Infra.Entities;
using Infra.Entities.HolidayTracking.Accounts;
using Infra.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Service.DTOs.HolidayDTOs;
using Service.DTOs.LoginDTOs;
using Service;
using Service.DTOs.UserDTOs;
using Service.Security;

namespace Tests.IntegrationTests.TestClasses;

public class AccountIntegrationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private readonly HelperMethods _helpers;

    public AccountIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _helpers = new HelperMethods(_factory, _client);
    }

    // --- CREATE TESTS ---

    #region Create Tests

    [Fact]
    public async Task CreateAccount_WithValidData_CreatesAccount()

    private async Task<string> GetAdminUserId()
    {
        using var scope = _factory.Services.CreateScope();

        var repo = scope.ServiceProvider.GetRequiredService<IUserRepo>();
        var admin = await repo.GetByUsername("integration_admin");

        return admin!.UserId;
    }

    private async Task<(string UserId, List<HolidayAccount> accounts)> CreateUserWithAccounts()
    {
        // Arrange
        await _helpers.AuthorizeAsAdmin();

        var userId = await _helpers.GetAdminUserId();

        var dto = new CreateHolidayAccountDto
        {
            UserId = userId,
            Type = HolidayType.Week1_5
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/CreateAccount",
            dto);
        using var scope = _factory.Services.CreateScope();

        var userService = scope.ServiceProvider.GetRequiredService<UserService>();

        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepo>();

        var accountRepo = scope.ServiceProvider.GetRequiredService<IAccountRepo>();

        var username = $"account_test_{Guid.NewGuid():N}";
    [Fact]
    public async Task CreateAccount_WithoutUserId_ReturnsBadRequest()
    {
        // Arrange
        await _helpers.AuthorizeAsAdmin();

        await userService.Insert(new CreateUserDto
        {
            Username = username,
            Password = "TestPassword123",
            IsAdmin = false
        });

        var user = await userRepo.GetByUsername(username);
    [Fact]
    public async Task CreateAccount_WithInvalidType_ReturnsBadRequest()
    {
        // Arrange
        await _helpers.AuthorizeAsAdmin();

        var accounts = await accountRepo.GetUserAccountsAsync(user!.UserId);
        var userId = await _helpers.GetAdminUserId();

        return (user.UserId, accounts.ToList());
    }

    #endregion

    // --- READ TESTS ---

    #region Read Tests

    [Fact]
    public async Task GetAccountById_WithExistingId_ReturnsAccount()
    {
        // Arrange
        await _helpers.AuthorizeAsAdmin();

        var userId = await _helpers.GetAdminUserId();
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
        await _helpers.AuthorizeAsAdmin();

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
        await _helpers.AuthorizeAsAdmin();

        var userId = await _helpers.GetAdminUserId();

        await _client.PostAsJsonAsync(
            "/CreateAccount",
            new CreateHolidayAccountDto
            {
                UserId = userId,
                Type = HolidayType.Week1_5
            });
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
        await _helpers.AuthorizeAsAdmin();

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