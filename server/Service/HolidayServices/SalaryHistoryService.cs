using Infra.Entities.HolidayTracking.Salary;
using Infra.Repositories.Interfaces;

namespace Service.HolidayServices;

public class SalaryHistoryService(ISalaryHistoryRepo repo)
{
    public async Task Create(SalaryHistory salaryHistory)
    {
        await repo.Insert(salaryHistory);
    }

    public async Task<List<SalaryHistory>> GetSalaryHistoryForUser(string userId)
    {
        return await repo.GetSalaryHistoryForUser(userId);
    }
    
}