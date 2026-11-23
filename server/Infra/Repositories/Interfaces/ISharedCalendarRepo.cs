using Infra.Entities.HolidayTracking.Requests;
using Infra.Entities.HolidayTracking.WorkSchedule;

namespace Infra.Repositories.Interfaces;

public interface ISharedCalendarRepo
{
    Task<List<HolidayRequest>> GetOverlapping(DateOnly start, DateOnly end, HolidayRequestStatus status);

}