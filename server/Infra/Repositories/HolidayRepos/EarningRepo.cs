using Infra.Entities.HolidayTracking.Earnings;
using Infra.Repositories.Interfaces;

namespace Infra.Repositories.HolidayRepos;


/**
 * Get and calculate the earnings
 */
public class EarningRepo(MyDatabaseConnection dc) : IEarningRepo
{
    public async Task SaveEarnings(HolidayEarning earning)
    {
        throw new NotImplementedException();
    }
}