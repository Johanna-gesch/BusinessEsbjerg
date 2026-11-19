using Infra.Entities.HolidayTracking.WorkSchedule;

namespace Infra.Repositories.Interfaces;

public interface IWorkScheduleRepo
{
    Task<WorkSchedule?> GetCurrent(string userId);
    Task<WorkScheduleHistory?> GetHistoryEffective(string userId, DateOnly date);
    Task<List<WorkScheduleHistory>> GetHistoryForUser(string userId);

    Task Insert(WorkScheduleHistory schedule);
}