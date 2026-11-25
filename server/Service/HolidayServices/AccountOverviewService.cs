using Infra.Repositories.Interfaces;
using Service.DTOs;

namespace Service.HolidayServices;

public class AccountOverviewService (IUserRepo userRepo, HolidayBalanceService balanceService)
{
    public async Task<List<EmployeebalanceDto>> GetOverview(DateOnly date)
    {
        var users = await userRepo.GetAll();

        var result = new List<EmployeebalanceDto>();

        foreach (var user in users.Where(u => u.IsActive).OrderBy(u => u.Username))
        {
            var balance = await balanceService.GetUserBalance(user.UserId, date);
            
            result.Add(new EmployeebalanceDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Week1_5Hours = balance.Week1_5Hours,
                Week6Hours = balance.Week6Hours
            });

        }

        return result;
    }
    
}