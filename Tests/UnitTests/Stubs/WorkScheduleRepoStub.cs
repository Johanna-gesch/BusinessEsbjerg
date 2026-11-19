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

    public Task Insert(WorkScheduleHistory schedule)
    {
        throw new NotImplementedException();
    }
}