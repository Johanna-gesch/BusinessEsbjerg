using Infra;
using Infra.Entities;
using Service;
using Service.DTOs.UserDTOs;
using Tests.UnitTests.Stubs;

namespace Tests.UnitTests;

public class UserServiceTests
{

    private readonly UserRepoStub _userRepo;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _userRepo = new UserRepoStub();
        _service = new UserService(_userRepo);
    }
    
    // *---* CREATE TESTS *---* //
    
    #region Create Tests
    
    [Fact]
    public async Task Insert_ValidUser_UserIsCreated()
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