using Infra.Entities.HolidayTracking.Accounts;
using Infra.Repositories.Interfaces;
using LinqToDB;
using LinqToDB.Async;

namespace Infra.Repositories.HolidayRepos;


/**
 * Get and create accounts
 */
public class AccountRepo(MyDatabaseConnection dc) : IAccountRepo
{
    public async Task InsertAsync(HolidayAccount account)
    {
        await dc.InsertAsync(account);
    }

    public async Task<HolidayAccount?> GetByIdAsync(Guid id)
    {
        return await dc.HolidayAccounts.FirstOrDefaultAsync(ha => ha.Id == id);
    }

    public async Task<ICollection<HolidayAccount>> GetUserAccountsAsync(string userId)
    {
        return await dc.HolidayAccounts
            .Where(ha => ha.UserId == userId)
            .ToListAsync();
    }
}