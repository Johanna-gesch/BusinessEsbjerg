
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Infra.Entities;
using Infra.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Service.DTOs.LoginDTOs;
using Service.Security;

namespace Tests.IntegrationTests;

public class UserIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public UserIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
    
    // *---* HELPER METHODS *---* //
    
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
                IsAdmin = true
            };

            await repo.Insert(admin);
        }

        var loginResponse = await _client.PostAsJsonAsync(
            "/login",
            new
            {
                Username = username,
                Password = password
            });

        loginResponse.EnsureSuccessStatusCode();

        var result = await loginResponse.Content
            .ReadFromJsonAsync<LoginResponseDTO>();

        return result!.Token;
    }
    
    private async Task AuthorizeAsAdmin()
    {
        var token = await LoginAsAdmin();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }
    #endregion

    // *---* CREATE TESTS *---* //
    
    #region Create Tests

    [Fact]
    public async Task CreateUser_WithoutUsername_ReturnsBadRequest()
    {
        // Arrange
        await AuthorizeAsAdmin();
        
        // Act
        var response = await _client.PostAsJsonAsync(
            "/CreateUser",
            new
            {
                Username = "",
                Password = "test123",
                IsAdmin = false
            }
        );

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithoutPassword_ReturnsBadRequest()
    {
        // Arrange
        await AuthorizeAsAdmin();
        
        // Act
        var response = await _client.PostAsJsonAsync(
            "/CreateUser",
            new
            {
                Username = "testuser",
                Password = "",
                IsAdmin = false
            }
        );

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithValidData_CreatesUser()
    {
        // Arrange
        await AuthorizeAsAdmin();
        
        var username = $"testuser_{Guid.NewGuid()}";

        // Act
        var response = await _client.PostAsJsonAsync(
            "/CreateUser",
            new
            {
                Username = username,
                Password = "test123",
                IsAdmin = false
            }
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Check that the user was actually saved
        var users = await _client.GetFromJsonAsync<List<User>>("/GetUsers");

        Assert.NotNull(users);
        Assert.Contains(users, user => user.Username == username);
    }
    #endregion


    // *---* READ TESTS *---* //
    
    #region Read Tests

    [Fact]
    public async Task GetUsers_ReturnsListOfUsers()
    {
        var response = await _client.GetAsync("/GetUsers");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var users = await response.Content.ReadFromJsonAsync<List<User>>();

        Assert.NotNull(users);
    }
    

    [Fact]
    public async Task GetUserById_WithExistingId_ReturnsUser()
    {
        // Arrange
        
        await AuthorizeAsAdmin();
        
        var username = $"testuser_{Guid.NewGuid()}";

        await _client.PostAsJsonAsync(
            "/CreateUser",
            new
            {
                Username = username,
                Password = "test123",
                IsAdmin = false
            }
        );

        var users = await _client.GetFromJsonAsync<List<User>>("/GetUsers");

        var createdUser = users!.First(user => user.Username == username);

        // Act
        var response = await _client.GetAsync(
            $"/GetUserById?id={createdUser.UserId}"
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var user = await response.Content.ReadFromJsonAsync<User>();

        Assert.NotNull(user);
        Assert.Equal(username, user.Username);
    }

    [Fact]
    public async Task GetUserById_WithUnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync(
            $"/GetUserById?id={Guid.NewGuid()}"
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    #endregion

    // *---* UPDATE TESTS *---* //
    
    #region Update Tests

    [Fact]
    public async Task UpdateUser_WithValidData_UpdatesUsername()
    {
        // Arrange
        await AuthorizeAsAdmin();
        
        var username = $"testuser_{Guid.NewGuid()}";

        await _client.PostAsJsonAsync(
            "/CreateUser",
            new
            {
                Username = username,
                Password = "test123",
                IsAdmin = false
            }
        );

        var users = await _client.GetFromJsonAsync<List<User>>("/GetUsers");

        var createdUser = users!.First(user => user.Username == username);

        var updatedUsername = $"updated_{Guid.NewGuid()}";

        // Act
        var response = await _client.PutAsJsonAsync(
            "/UpdateUser",
            new
            {
                UserId = createdUser.UserId,
                Username = updatedUsername
            }
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedUser = await _client.GetFromJsonAsync<User>(
            $"/GetUserById?id={createdUser.UserId}"
        );

        Assert.NotNull(updatedUser);
        Assert.Equal(updatedUsername, updatedUser.Username);
    }
    #endregion
    
    // *---* LOGIN TESTS *---* //
    
    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var token = await LoginAsAdmin();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        // Arrange
        await LoginAsAdmin();

        // Act
        var response = await _client.PostAsJsonAsync(
            "/login",
            new
            {
                Username = "integration_admin",
                Password = "WrongPassword123"
            }
        );

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownUsername_ReturnsUnauthorized()
    {
        // Arrange
        var response = await _client.PostAsJsonAsync(
            "/login",
            new
            {
                Username = "unknown_user",
                Password = "Password123"
            }
        );

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion
}
