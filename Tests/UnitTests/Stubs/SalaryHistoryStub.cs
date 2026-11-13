using Infra.Entities.HolidayTracking.Salary;
using Infra.Repositories.Interfaces;

namespace Tests.UnitTests.Stubs;

public class SalaryHistoryStub : ISalaryHistoryRepo
{
    private readonly List<SalaryHistory> _salHis = new();
    public async Task Insert(SalaryHistory salaryHistory)
    {
        _salHis.Add(salaryHistory);
        await Task.CompletedTask;
    }

    public async Task<List<SalaryHistory>> GetSalaryHistoryForUser(string userId)
    {
        return (await Task.FromResult(_salHis.Where(h => h.UserId == userId))).ToList();
    }
    
    
    
}