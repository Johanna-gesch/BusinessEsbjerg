
using Infra.Entities;
using Service;
using Tests.UnitTests.Stubs;

namespace Tests.UnitTests;

public class AuthenticationServiceTests
{
    private readonly UserRepoStub _userRepo = new();
    private readonly HasherStub _hasher = new();
    private readonly TokenServiceStub _tokenServiceStub = new();

    private AuthenticationService CreateService()
    {
        return new AuthenticationService(
            _tokenServiceStub,
            _userRepo,
            _hasher
        );
    }

    [Fact]
    public async Task Login_ReturnsToken_WhenCredentialsAreValid()
    {
        // Arrange
        var user = new User
        {
            UserId = "123",
            Username = "testuser",
            PasswordHash = _hasher.HashAndSaltPassword("password123"),
            IsAdmin = false
        };

        await _userRepo.Insert(user);

        var service = CreateService();

        // Act
        var result = await service.Login("testuser", "password123");

        // Assert
        Assert.Equal("fake-jwt-token", result);
        Assert.Equal(1, _tokenServiceStub.GenerateTokenCallCount);
        Assert.Same(user, _tokenServiceStub.GeneratedForUser);
    }

    [Fact]
    public async Task Login_ReturnsNull_WhenUserDoesNotExist()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.Login("unknown", "password123");

        // Assert
        Assert.Null(result);
        Assert.Equal(0, _tokenServiceStub.GenerateTokenCallCount);
    }

    [Fact]
    public async Task Login_ReturnsNull_WhenPasswordIsIncorrect()
    {
        // Arrange
        var user = new User
        {
            UserId = "123",
            Username = "testuser",
            PasswordHash = _hasher.HashAndSaltPassword("correctPassword"),
            IsAdmin = false
        };

        await _userRepo.Insert(user);

        var service = CreateService();

        // Act
        var result = await service.Login("testuser", "wrongPassword");

        // Assert
        Assert.Null(result);
        Assert.Equal(0, _tokenServiceStub.GenerateTokenCallCount);
    }
}
