using Infra;
using Infra.Entities;
using Infra.Entities.HolidayTracking;
using Infra.Entities.HolidayTracking.Accounts;
using Infra.Entities.HolidayTracking.Earnings;
using Infra.Entities.HolidayTracking.Requests;
using Infra.Entities.HolidayTracking.Rules;
using Infra.Entities.HolidayTracking.Salary;
using Infra.Entities.HolidayTracking.Transactions;
using Infra.Entities.HolidayTracking.WorkSchedule;
using LinqToDB;
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

        dc.CreateTable<SalaryHistory>(tableOptions: TableOptions.CreateIfNotExists);

        dc.CreateTable<RuleParameter>(tableOptions: TableOptions.CreateIfNotExists);

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
                    IsAdmin = i == 1,
                    IsActive = true
                });
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
                IsAdmin = true,
                IsActive = true
            });
        }
    }
    
}