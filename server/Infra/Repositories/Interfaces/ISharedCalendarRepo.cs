using Infra.Entities.HolidayTracking.WorkSchedule;

namespace Infra.Repositories.Interfaces;

public interface ISharedCalendarRepo
{
    Task<List<WorkScheduleHistory>> GetAllHistoryUpTo(DateOnly end);

}