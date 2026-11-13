
using Infra;
using Infra.Entities;
using Infra.Entities.HolidayTracking.Accounts;
using Service.DTOs.HolidayDTOs;
using Service.HolidayServices;
using Tests.UnitTests.Stubs;

namespace Tests.UnitTests;

public class AccountServiceTests
{
    private readonly AccountService _service;
    private readonly AccountRepoStub _accountRepo = new();

    public AccountServiceTests()
    {
        _service = new AccountService(_accountRepo);
    }

    // --- CREATE TESTS ---

    #region Create Tests

    [Fact]
    public async Task CreateDefaultAccounts_ValidUserId_CreatesTwoAccounts()
    {
        // Arrange
        var userId = "123";

        // Act
        await _service.CreateDefaultAccountsAsync(userId);

        // Assert
        var accounts = await _accountRepo.GetUserAccountsAsync(userId);

        Assert.Equal(2, accounts.Count);
        Assert.Contains(accounts, a => a.Type == HolidayType.Week1_5);
        Assert.Contains(accounts, a => a.Type == HolidayType.Week6);

        Assert.All(accounts, account =>
            Assert.Equal(userId, account.UserId));

        Assert.All(accounts, account =>
            Assert.NotEqual(Guid.Empty, account.Id));
    }

    [Fact]
    public async Task CreateDefaultAccounts_EmptyUserId_ThrowsValidationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateDefaultAccountsAsync(""));
    }

    [Fact]
    public async Task CreateDefaultAccounts_NullUserId_ThrowsValidationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateDefaultAccountsAsync(null!));
    }

    #endregion

    // --- READ TESTS ---

    #region Read Tests

    [Fact]
    public async Task GetAccountById_ExistingAccount_ReturnsAccount()
    {
        // Arrange
        var account = new HolidayAccount
        {
            Id = Guid.NewGuid(),
            UserId = "123",
            Type = HolidayType.Week1_5
        };

        await _accountRepo.InsertAsync(account);

        // Act
        var result = await _service.GetAccountById(account.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(account.Id, result.Id);
        Assert.Equal("123", result.UserId);
    }

    [Fact]
    public async Task GetAccountById_EmptyId_ThrowsValidationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.GetAccountById(Guid.Empty));
    }

    [Fact]
    public async Task GetAccountById_UnknownId_ThrowsNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.GetAccountById(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAccountsForUser_ExistingAccounts_ReturnsUserAccounts()
    {
        // Arrange
        await _accountRepo.InsertAsync(new HolidayAccount
        {
            Id = Guid.NewGuid(),
            UserId = "123",
            Type = HolidayType.Week1_5
        });

        await _accountRepo.InsertAsync(new HolidayAccount
        {
            Id = Guid.NewGuid(),
            UserId = "456",
            Type = HolidayType.Week1_5
        });

        // Act
        var result = await _service.GetAccountsForUserAsync("123");

        // Assert
        Assert.Single(result);
        Assert.All(result, account => Assert.Equal("123", account.UserId));
    }

    [Fact]
    public async Task GetAccountsForUser_EmptyUserId_ThrowsValidationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.GetAccountsForUserAsync(""));
    }

    #endregion
}