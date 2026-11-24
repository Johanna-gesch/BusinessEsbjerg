using Infra.Entities.HolidayTracking.Earnings;

namespace Infra.Repositories.Interfaces;

public interface IEarningRepo
{
    Task SaveEarningsTransaction(List<HolidayEarning> earnings);
    
    Task<List<HolidayEarning>> GetEarningsByUserAndAccount(string userId, Guid accountId);
    
    Task<bool> EarningsExist(Guid accountId, DateOnly earnedDate);

    
}