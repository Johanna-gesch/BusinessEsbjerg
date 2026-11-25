using System.Net.Http.Headers;
using System.Net.Http.Json;
using Infra;
using Infra.Entities;
using Infra.Entities.HolidayTracking.Accounts;
using Infra.Entities.HolidayTracking.Earnings;
using Infra.Entities.HolidayTracking.WorkSchedule;
using Infra.Entities.Users;
using Infra.Repositories.Interfaces;
using LinqToDB;
using LinqToDB.Async;
using Microsoft.AspNetCore.Mvc.Testing;
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
        var accountRepo = scope.ServiceProvider.GetRequiredService<IAccountRepo>();

        var admin = await repo.GetByUsername(username);

        if (admin == null)
        {
            admin = new User
            {
                UserId = Guid.NewGuid().ToString(),
                Username = username,
                PasswordHash = hasher.HashAndSaltPassword(password),
                Role = UserRole.Admin,
                IsActive = true
            };

            await repo.Insert(admin);
        }
        
        // Get existing accounts for the admin
        var accounts = await accountRepo.GetUserAccountsAsync(admin.UserId);
        
        // Ensure Week1_5 account exists
        if (!accounts.Any(a => a.Type == HolidayType.Week1_5))
        {
            await accountRepo.InsertAsync(new HolidayAccount
            {
                Id = Guid.NewGuid(),
                UserId = admin.UserId,
                Type = HolidayType.Week1_5
            });
        }

        // Ensure Week6 account exists
        if (!accounts.Any(a => a.Type == HolidayType.Week6))
        {
            await accountRepo.InsertAsync(new HolidayAccount
            {
                Id = Guid.NewGuid(),
                UserId = admin.UserId,
                Type = HolidayType.Week6
            });
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
            Role = UserRole.User
        });

        var user = await userRepo.GetByUsername(username);

        var accounts = await accountRepo.GetUserAccountsAsync(user!.UserId);

        return (user.UserId, accounts.ToList());
    }
    
    public async Task CreateWorkScheduleHistory(
        string userId,
        DateOnly effectiveFrom,
        decimal mondayHours,
        decimal tuesdayHours,
        decimal wednesdayHours,
        decimal thursdayHours,
        decimal fridayHours)
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<MyDatabaseConnection>();

        await db.InsertAsync(new WorkScheduleHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EffectiveFrom = effectiveFrom,
            MondayHours = mondayHours,
            TuesdayHours = tuesdayHours,
            WednesdayHours = wednesdayHours,
            ThursdayHours = thursdayHours,
            FridayHours = fridayHours
        });
    }

    public async Task RemoveUserWithAccounts(string userId)
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<MyDatabaseConnection>();
        
        
        await db.GetTable<HolidayAccount>()
            .Where(a => a.UserId == userId)
            .DeleteAsync();
        
        await db.GetTable<User>()
            .Where(u => u.UserId == userId)
            .DeleteAsync();
    }

    public async Task RemoveUserWithAccountsAndSchedulesAndEarnings(string userId)
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<MyDatabaseConnection>();
        
        // Get all account IDs belonging to the user.
        var accountIds = await db.GetTable<HolidayAccount>()
            .Where(a => a.UserId == userId)
            .Select(a => a.Id)
            .ToListAsync();
        
        // Delete all earnings belonging to the user's accounts.
        if (accountIds.Count > 0)
        {
            await db.GetTable<HolidayEarning>()
                .Where(e => accountIds.Contains(e.HolidayAccountId))
                .DeleteAsync();
        }
        
        // Delete the user's holiday accounts.
        await db.GetTable<HolidayAccount>()
            .Where(a => a.UserId == userId)
            .DeleteAsync();

        // Delete the user's work schedule history.
        await db.GetTable<WorkScheduleHistory>()
            .Where(s => s.UserId == userId)
            .DeleteAsync();

        // Delete the user.
        await db.GetTable<User>()
            .Where(u => u.UserId == userId)
            .DeleteAsync();
    }
    
}