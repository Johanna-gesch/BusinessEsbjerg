using Infra.Entities.HolidayTracking.Earnings;

namespace Infra.Repositories.Interfaces;

public interface IEarningRepo
{
    Task SaveEarningsTransaction(List<HolidayEarning> earnings);
    
    Task<List<HolidayEarning>> GetEarningsByAccount(Guid accountId);
    
    Task<bool> EarningsExist(Guid accountId, DateOnly earnedDate);


}