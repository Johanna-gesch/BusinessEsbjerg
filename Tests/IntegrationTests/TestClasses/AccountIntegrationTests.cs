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

    // --- READ TESTS ---

    #region Read Tests

    [Fact]
    public async Task GetAccountById_WithExistingId_ReturnsAccount()
    {
        // Arrange
        await _helpers.AuthorizeAsAdmin();

        var (userId, accounts) = await _helpers.CreateUserWithAccounts();

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

        var (userId, _) = await _helpers.CreateUserWithAccounts();

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