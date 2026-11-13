using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Infra.Entities;
using Infra.Entities.HolidayTracking.Accounts;
using Infra.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Service.DTOs.HolidayDTOs;
using Service.DTOs.LoginDTOs;
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

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var account = await response.Content.ReadFromJsonAsync<HolidayAccount>();

        Assert.NotNull(account);
        Assert.NotEqual(Guid.Empty, account.Id);
        Assert.Equal(userId, account.UserId);
        Assert.Equal(dto.Type, account.Type);
    }

    [Fact]
    public async Task CreateAccount_WithoutUserId_ReturnsBadRequest()
    {
        // Arrange
        await _helpers.AuthorizeAsAdmin();

        var dto = new CreateHolidayAccountDto
        {
            UserId = "",
            Type = HolidayType.Week1_5
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/CreateAccount",
            dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAccount_WithInvalidType_ReturnsBadRequest()
    {
        // Arrange
        await _helpers.AuthorizeAsAdmin();

        var userId = await _helpers.GetAdminUserId();

        var dto = new CreateHolidayAccountDto
        {
            UserId = userId,
            Type = (HolidayType)999
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/CreateAccount",
            dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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

        var createResponse = await _client.PostAsJsonAsync(
            "/CreateAccount",
            new CreateHolidayAccountDto
            {
                UserId = userId,
                Type = HolidayType.Week1_5
            });
        
        var createdAccount =
            await createResponse.Content.ReadFromJsonAsync<HolidayAccount>();

        Assert.NotNull(createdAccount);

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

        // Act
        var response = await _client.GetAsync(
            $"/GetAccountsForUser?userId={Uri.EscapeDataString(userId)}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var accounts =
            await response.Content.ReadFromJsonAsync<List<HolidayAccount>>();

        Assert.NotNull(accounts);
        Assert.NotEmpty(accounts);
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