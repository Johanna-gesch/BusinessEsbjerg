using Infra.Entities.HolidayTracking.WorkSchedule;

namespace Infra.Repositories.Interfaces;

public interface IWorkScheduleRepo
{
    Task<WorkSchedule?> GetCurrent(string userId);
    Task<WorkScheduleHistory?> GetHistoryEffective(string userId, DateOnly date);
    Task<List<WorkScheduleHistory>> GetHistoryForUser(string userId);
    Task<List<WorkScheduleHistory>> GetSchedulesForPeriod(string userId, DateOnly start, DateOnly end);

    Task Insert(WorkScheduleHistory schedule);
    
    Task<List<WorkScheduleHistory>> GetAllHistoryUpTo(DateOnly end);
}