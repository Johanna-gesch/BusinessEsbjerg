using Infra.Entities.HolidayTracking.Transactions;
using Infra.Repositories.Interfaces;
using LinqToDB;
using LinqToDB.Async;

namespace Infra.Repositories.HolidayRepos;


/**
 * Get and set transactions
 */
public class TransactionRepo(MyDatabaseConnection dc) : ITransactionRepo
{
    public async Task<List<HolidayTransaction>> GetByAccount(Guid accountId)
    {
        return await dc.GetTable<HolidayTransaction>()
            .Where(t => t.HolidayAccountId == accountId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task Insert(HolidayTransaction transaction)
    {
        await dc.InsertAsync(transaction);
    }
}