
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

    /*
     * This method is for retrieving the schedule for a period of time.
     * It is used for checking how many hours must be spent during a holiday.
     * It checks the work schedule that is active at the start of the request
     * and the schedule that is active at the end. 
     */
    public async Task<List<WorkScheduleHistory>> GetSchedulesForPeriod(string userId, DateOnly start, DateOnly end)
    {
        var schedules = await dc.GetTable<WorkScheduleHistory>()
            .Where(h => h.UserId == userId && h.EffectiveFrom <= end)
            .OrderByDescending(h => h.EffectiveFrom)
            .ToListAsync();

        var effectiveAtStart = schedules
            .FirstOrDefault(h => h.EffectiveFrom <= start);

        if (effectiveAtStart == null)
            throw new NotFoundException($"No work schedule exists for user {userId} on {start}.");

        return schedules
            .Where(h => h.EffectiveFrom >= start)
            .Append(effectiveAtStart)
            .DistinctBy(h => h.EffectiveFrom)
            .OrderBy(h => h.EffectiveFrom)
            .ToList();
    }

    public async Task Insert(WorkScheduleHistory schedule)
    {
        await dc.InsertAsync(schedule);
    }
    
    public async Task<List<WorkScheduleHistory>> GetAllHistoryUpTo(DateOnly end)
    {
        return await dc.GetTable<WorkScheduleHistory>()
            .Where(h => h.EffectiveFrom <= end)
            .ToListAsync();
    }
}