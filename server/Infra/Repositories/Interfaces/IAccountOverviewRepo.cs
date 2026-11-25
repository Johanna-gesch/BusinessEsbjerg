using Infra.Entities.HolidayTracking.Accounts;
using Infra.Entities.HolidayTracking.Earnings;
using Infra.Entities.HolidayTracking.Transactions;

namespace Infra.Repositories.Interfaces;


// Reads the holiday data for all employees at once.
public interface IAccountOverviewRepo
{
    Task<List<HolidayAccount>> GetAccounts();

    Task<List<HolidayEarning>> GetEarningsValidOn(DateOnly date);

    Task<List<HolidayTransaction>> GetEarningTransactionsUpTo(DateOnly date);

}