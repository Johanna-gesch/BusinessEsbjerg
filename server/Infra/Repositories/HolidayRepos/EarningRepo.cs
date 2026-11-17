using Infra.Entities.HolidayTracking.Earnings;
using Infra.Repositories.Interfaces;
using LinqToDB;
using LinqToDB.Async;

namespace Infra.Repositories.HolidayRepos;


/**
 * Get and calculate the earnings
 */
public class EarningRepo(MyDatabaseConnection dc) : IEarningRepo
{
    
    public async Task SaveEarningsTransaction(List<HolidayEarning> earnings)
    {
        using var transaction = dc.BeginTransaction();

        try
        {
            foreach (var earning in earnings)
            {
                await dc.InsertAsync(earning);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<List<HolidayEarning>> GetEarningsByUserAndAccount(
        string userId,
        Guid accountId)
    {
        return await dc.HolidayEarnings
            .Where(e =>
                e.UserId == userId &&
                e.HolidayAccountId == accountId)
            .ToListAsync();
    }

    public async Task<bool> EarningsExist(Guid accountId, DateOnly earnedDate)
    {
        return await dc.HolidayEarnings
            .AnyAsync(e =>
                e.HolidayAccountId == accountId &&
                e.EarnedDate == earnedDate
            );
    }

}