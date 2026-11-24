using Infra.Entities.HolidayTracking.WorkSchedule;
using Infra.Repositories.Interfaces;

namespace Tests.UnitTests.Stubs;

public class WorkScheduleRepoStub : IWorkScheduleRepo
{
    public List<WorkSchedule> Current { get; } = new(); 
    public List<WorkScheduleHistory> History { get; } = new();


    public Task<WorkSchedule?> GetCurrent(string userId)
    {
       return Task.FromResult(Current.FirstOrDefault(s => s.UserId == userId));
    }

    public Task<WorkScheduleHistory?> GetHistoryEffective(string userId, DateOnly date)
    {
        return Task.FromResult(History
            .Where(h => h.UserId == userId && h.EffectiveFrom <= date)
            .OrderByDescending(h => h.EffectiveFrom)
            .FirstOrDefault());
    }

    public Task<List<WorkScheduleHistory>> GetHistoryForUser(string userId)
    {
        return Task.FromResult(History
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.EffectiveFrom)
            .ToList());
    }

    public Task<List<WorkScheduleHistory>> GetSchedulesForPeriod(string userId, DateOnly start, DateOnly end)
    {
        // Get all schedules that have become effective by the end date.
        var schedules = History
            .Where(h => h.UserId == userId && h.EffectiveFrom <= end)
            .OrderBy(h => h.EffectiveFrom)
            .ToList();

        // Include the most recent schedule before the request starts
        // because that schedule may still apply during the request.
        var previousSchedule = schedules
            .LastOrDefault(h => h.EffectiveFrom <= start);

        var result = schedules
            .Where(h => h.EffectiveFrom > start)
            .ToList();

        if (previousSchedule != null)
            result.Insert(0, previousSchedule);

        return Task.FromResult(result);
    }

    public Task Insert(WorkScheduleHistory schedule)
    {
        History.Add(schedule);
        return Task.CompletedTask;
    }

    public Task<List<WorkScheduleHistory>> GetAllHistoryUpTo(DateOnly end)
    {
        var schedules = History
            .Where(h => h.EffectiveFrom <= end)
            .ToList();

        return Task.FromResult(schedules);
    }
}