
using Infra.Entities.HolidayTracking.WorkSchedule;
using Infra.Repositories.Interfaces;
using LinqToDB;
using LinqToDB.Async;

namespace Infra.Repositories.HolidayRepos;

/**
 * Get and set the work schedules.
 */
public class WorkScheduleRepo(MyDatabaseConnection dc) : IWorkScheduleRepo
{
    public async Task<WorkSchedule?> GetCurrent(string userId)
    {
        return await dc.GetTable<WorkSchedule>()
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<WorkScheduleHistory?> GetHistoryEffective(string userId, DateOnly date)
    {
        return await dc.GetTable<WorkScheduleHistory>()
            .Where(h => h.UserId == userId && h.EffectiveFrom <= date)
            .OrderByDescending(h => h.EffectiveFrom)
            .FirstOrDefaultAsync();
    }

    public async Task<List<WorkScheduleHistory>> GetHistoryForUser(string userId)
    {
        return await dc.GetTable<WorkScheduleHistory>()
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.EffectiveFrom)
            .ToListAsync();
    }
}