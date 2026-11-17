using Infra;
using Infra.Entities.HolidayTracking;
using Infra.Entities.HolidayTracking.Accounts;
using Infra.Entities.HolidayTracking.Earnings;
using Infra.Entities.HolidayTracking.Requests;
using Infra.Entities.HolidayTracking.Transactions;
using Infra.Entities.HolidayTracking.WorkSchedule;
using Infra.Entities.Users;
using LinqToDB;
using Microsoft.AspNetCore.Identity;
using Service.Security;

namespace API;

public class MySeeder(MyDatabaseConnection dc, IPasswordHasher hasher)
{

    private void InitializeDatabase()
    {
        dc.CreateTable<User>(tableOptions: TableOptions.CreateIfNotExists);

        dc.CreateTable<HolidayAccount>(tableOptions: TableOptions.CreateIfNotExists);

        dc.CreateTable<HolidayEarning>(tableOptions: TableOptions.CreateIfNotExists);

        dc.CreateTable<HolidayTransaction>(tableOptions: TableOptions.CreateIfNotExists);

        dc.CreateTable<HolidayRequest>(tableOptions: TableOptions.CreateIfNotExists);

        dc.CreateTable<WorkSchedule>(tableOptions: TableOptions.CreateIfNotExists);

        dc.CreateTable<WorkScheduleHistory>(tableOptions: TableOptions.CreateIfNotExists);

        dc.CreateTable<CalendarDay>(tableOptions: TableOptions.CreateIfNotExists);
    }
    public void Seed()
    {
        InitializeDatabase();
        
        if (dc.Users.Count() == 0)
            for (int i = 1; i <= 10; i++)
            {
                dc.Insert(new User
                {
                    UserId = i.ToString(),
                    Username = $"User {i}",
                    Role = 0,
                    IsActive = true
                });
            }
        
        var users = dc.Users.ToList();
        
        foreach (var user in users)
        {
            EnsureHolidayAccounts(user.UserId);
        }

        if (dc.WorkScheduleHistories.Count() == 0)
        {
            // Hent alle brugere færdigt, inden vi begynder at indsætte.
            
            foreach (var user in users)
            {
                dc.Insert(new WorkScheduleHistory
                {
                    Id = Guid.NewGuid(),
                    EffectiveFrom = new DateOnly(2026, 09, 05),
                    UserId = $"{user.UserId}",
                    MondayHours = 7.5m,
                    TuesdayHours = 7.5m,
                    WednesdayHours = 7.5m,
                    ThursdayHours = 7.5m,
                    FridayHours = 7
                });
            }
        }
            
    }

    public void SeedForTests()
    {
        InitializeDatabase();

        const string username = "playwright_admin";
        const string password = "PlaywrightPassword123";

        // Idempotence: Only create the user if it doesn't already exist.
        if (!dc.Users.Any(u => u.Username == username))
        {
            dc.Insert(new User
            {
                UserId = Guid.NewGuid().ToString(),
                Username = username,
                PasswordHash = hasher.HashAndSaltPassword(password),
                Role = 0,
                IsActive = true
            });
        }
    }
    
    private void EnsureHolidayAccounts(string userId)
    {
        var existingAccounts = dc.HolidayAccounts
            .Where(a => a.UserId == userId)
            .ToList();

        if (!existingAccounts.Any(a => a.Type == HolidayType.Week1_5))
        {
            dc.Insert(new HolidayAccount
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = HolidayType.Week1_5
            });
        }

        if (!existingAccounts.Any(a => a.Type == HolidayType.Week6))
        {
            dc.Insert(new HolidayAccount
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = HolidayType.Week6
            });
        }
    }
    
}