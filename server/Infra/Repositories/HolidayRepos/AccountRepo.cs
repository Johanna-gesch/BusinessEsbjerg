using Infra.Entities.HolidayTracking.Accounts;

namespace Infra.Repositories.HolidayRepos;


/**
 * Get and create accounts
 */
public class AccountRepo(MyDatabaseConnection dc)
{
    public async Task InsertAccount(HolidayAccount account)
    {
        throw new NotImplementedException();
    }
}