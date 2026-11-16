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

public class HelperMethods
{
    
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public HelperMethods(CustomWebApplicationFactory factory, HttpClient client)
    {
        _factory = factory;
        _client = client;
    }
    
    public async Task AuthorizeAsAdmin()
    {
        var token = await LoginAsAdmin();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }
    
    public async Task<string> LoginAsAdmin()
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
    
    public async Task<string> GetAdminUserId()
    {
        using var scope = _factory.Services.CreateScope();

        var repo = scope.ServiceProvider.GetRequiredService<IUserRepo>();
        var admin = await repo.GetByUsername("integration_admin");

        return admin!.UserId;
    }
    
    public async Task<(string UserId, List<HolidayAccount> accounts)> CreateUserWithAccounts()
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
}