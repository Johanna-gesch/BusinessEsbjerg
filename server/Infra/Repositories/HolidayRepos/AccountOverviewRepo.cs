using Infra.Entities.HolidayTracking.Accounts;
using Infra.Entities.HolidayTracking.Earnings;
using Infra.Entities.HolidayTracking.Transactions;
using Infra.Repositories.Interfaces;
using LinqToDB.Async;

namespace Infra.Repositories.HolidayRepos;


// Reads accounts, earnings and transactions for all employees with one query each.
// the filtering happens in the queries.
public class AccountOverviewRepo (MyDatabaseConnection dc) : IAccountOverviewRepo
{
    public async Task<List<HolidayAccount>> GetAccounts()
    {
        return await dc.HolidayAccounts.ToListAsync();
    }

    public async Task<List<HolidayEarning>> GetEarningsValidOn(DateOnly date)
    {
        return await dc.HolidayEarnings
            .Where(e => e.EarnedDate <= date && e.ExpiryDate >= date)
            .ToListAsync();
    }

    public async Task<List<HolidayTransaction>> GetEarningTransactionsUpTo(DateOnly date)
    {
        return await dc.HolidayTransactions
            .Where(t => t.HolidayEarningId != null && t.TransactionDate <= date)
            .ToListAsync();
    }
}