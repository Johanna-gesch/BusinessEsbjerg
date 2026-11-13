using Infra;
using Infra.Entities;
using Service;
using Service.DTOs.UserDTOs;
using Service.HolidayServices;
using Service.Security;
using Tests.UnitTests.Stubs;

namespace Tests.UnitTests;

public class UserServiceTests
{
    
    private readonly UserService _service;
    private readonly AccountService _accountService;
    private readonly UserRepoStub _userRepo = new();
    private readonly AccountRepoStub _accountRepoStub = new();
    private readonly HasherStub _hasher = new();

    public UserServiceTests()
    {
        _accountService = new AccountService(_accountRepoStub);
        _service = new UserService(_userRepo, _hasher, _accountService);
    }
    
    // *---* CREATE TESTS *---* //
    
    #region Create Tests
    
    [Fact]
    public async Task Insert_ValidUser_UserIsCreatedWithHashedPassword()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Username = "TestUser",
            Password = "TestPassword",
            IsAdmin = false
        };
        
        //Act
        await _service.Insert(dto);
        
        //Assert
        var users = await _userRepo.GetAll();
        
        Assert.Single(users); //Only 1 user created
        Assert.Equal("TestUser", users.First().Username);
        Assert.False(users.First().IsAdmin);
        Assert.False(string.IsNullOrWhiteSpace(users.First().UserId));
        
        // Password should be hashed before saving
        Assert.Equal("hashed_TestPassword", users.First().PasswordHash);
        Assert.NotEqual("TestPassword", users.First().PasswordHash);
    }
    
    [Fact]
    public async Task Insert_EmptyUsername_ThrowsValidationException()
    {
        
        // Arrange
        var dto = new CreateUserDto
        {
            Username = "",
            Password = "Password123",
        };
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _service.Insert(dto));
    }
    
    [Fact]
    public async Task Insert_EmptyPassword_ThrowsValidationException()
    {
        
        // Arrange
        var dto = new CreateUserDto
        {
            Username = "TestUser",
            Password = "",
        };
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _service.Insert(dto));
    }
    
    [Fact]
    public async Task Insert_ValidUser_CreatesTwoHolidayAccounts()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Username = "TestUser",
            Password = "TestPassword",
            IsAdmin = false
        };

        // Act
        await _service.Insert(dto);

        // Assert
        var user = Assert.Single(await _userRepo.GetAll());

        Assert.Equal(2, _accountRepoStub.Accounts.Count);

        Assert.All(_accountRepoStub.Accounts, account =>
            Assert.Equal(user.UserId, account.UserId));

        Assert.Contains(_accountRepoStub.Accounts,
            account => account.Type == HolidayType.Week1_5);

        Assert.Contains(_accountRepoStub.Accounts,
            account => account.Type == HolidayType.Week6);
    }
    
    
    #endregion
    
    // *---* PASSWORD TESTS *---* //
    
    #region Password Tests
    
    [Fact]
    public void HashAndSaltPassword_ValidPassword_ReturnsHashedPassword()
    {
        // Arrange
        string password = "TestPassword123";

        // Act
        string hashedPassword = _hasher.HashAndSaltPassword(password);

        // Assert
        Assert.NotEqual(password, hashedPassword);
        Assert.Contains(".", hashedPassword);
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        string password = "TestPassword123";
        string hashedPassword = _hasher.HashAndSaltPassword(password);

        // Act
        bool result = _hasher.VerifyPassword(password, hashedPassword);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        // Arrange
        string password = "TestPassword123";
        string hashedPassword = _hasher.HashAndSaltPassword(password);

        // Act
        bool result = _hasher.VerifyPassword("WrongPassword", hashedPassword);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HashAndSaltPassword_SamePassword_GeneratesDifferentHashes()
    {
        // Arrange
        string password = "TestPassword123";

        // Act
        string hash1 = _hasher.HashAndSaltPassword(password);
        string hash2 = _hasher.HashAndSaltPassword(password);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }
    
    #endregion
    
    // *---* READ TESTS *---* //
    
    #region Read Tests
    
    [Fact]
    public async Task GetById_ExistingUser_ReturnsUser()
    {
        // Arrange
        var user = new User
        {
            UserId = "123",
            Username = "TestUser",
            PasswordHash = "TestHash",
        };
        
        await _userRepo.Insert(user);
        
        // Act
        var result = await _service.GetById("123");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("123", result.UserId);
        Assert.Equal("TestUser", result.Username);
    }

    [Fact]
    public async Task GetById_EmptyId_ThrowsValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() => _service.GetById(""));
    }

    [Fact]
    public async Task GetById_UnknownId_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetById("unknown"));
    }
    #endregion
    
    // *---* UPDATE TESTS *---* //
    
    #region Update Tests
    
    [Fact]
    public async Task Update_ValidUser_UsernameIsUpdated()
    {
        // Arrange
        var user = new User
        {
            UserId = "123",
            Username = "OldUsername",
            PasswordHash = "TestHash",
        };
        
        await _userRepo.Insert(user);

        var dto = new UpdateUserDto
        {
            UserId = "123",
            Username = "NewUsername",
        };
        
        // Act
        await _service.Update(dto);
        
        //Assert
        var result = await _userRepo.GetById("123");
        
        Assert.NotNull(result);
        Assert.Equal("NewUsername", result.Username);
    }
    #endregion
    
    
}