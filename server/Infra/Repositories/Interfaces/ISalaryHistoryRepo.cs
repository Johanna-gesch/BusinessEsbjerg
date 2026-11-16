using Infra.Entities.HolidayTracking.Salary;

namespace Infra.Repositories.Interfaces;

public interface ISalaryHistoryRepo
{
    Task Insert(SalaryHistory salaryHistory);
    Task<List<SalaryHistory>> GetSalaryHistoryForUser(string userId);

}