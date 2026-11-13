
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
    public async Task CreateAccount_ValidAccount_AccountIsCreated()
    {
        // Arrange
        var dto = new CreateHolidayAccountDto
        {
            UserId = "123",
            Type = HolidayType.Week1_5
        };

        // Act
        var result = await _service.CreateAccountAsync(dto);

        // Assert
        var accounts = await _accountRepo.GetUserAccountsAsync("123");

        Assert.Single(accounts);
        Assert.Equal("123", result.UserId);
        Assert.Equal(dto.Type, result.Type);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task CreateAccount_EmptyUserId_ThrowsValidationException()
    {
        // Arrange
        var dto = new CreateHolidayAccountDto
        {
            UserId = "",
            Type = HolidayType.Week1_5
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateAccountAsync(dto));
    }

    [Fact]
    public async Task CreateAccount_InvalidType_ThrowsValidationException()
    {
        // Arrange
        var dto = new CreateHolidayAccountDto
        {
            UserId = "123",
            Type = (HolidayType)999
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateAccountAsync(dto));
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