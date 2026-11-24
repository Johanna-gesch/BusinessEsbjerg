using Infra.Entities.HolidayTracking.Requests;
using Infra.Entities.HolidayTracking.WorkSchedule;

namespace Infra.Repositories.Interfaces;

/*
 * Reads holiday requests for the shared calendar.
 */
public interface ISharedCalendarRepo
{
    // Gets all requests with the given status that overlap the period.
    Task<List<HolidayRequest>> GetOverlapping(DateOnly start, DateOnly end, HolidayRequestStatus status);

}