using Infra.Entities.HolidayTracking.Salary;
using Infra.Repositories.Interfaces;
using LinqToDB;
using LinqToDB.Async;

namespace Infra.Repositories.HolidayRepos;

/*
 * Get salary history
 */
public class SalaryHistoryRepo(MyDatabaseConnection dc) : ISalaryHistoryRepo
{
    public async Task Insert(SalaryHistory salaryHistory)
    {
        await dc.InsertAsync(salaryHistory);
    }

    public async Task<List<SalaryHistory>> GetSalaryHistoryForUser(string userId)
    {
        return await dc.SalaryHistories
            .Where(h => h.UserId == userId)
            .ToListAsync();
    }
}