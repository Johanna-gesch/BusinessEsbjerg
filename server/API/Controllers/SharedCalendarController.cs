using Infra.Entities.HolidayTracking.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs.SharedCalendarDTos;
using Service.HolidayServices;

namespace API.Controllers;

/*
 * Endpoint for the shared calendar. Any logged in user can view it
 */

[ApiController]
public class SharedCalendarController (SharedCalendarService service) : ControllerBase
{
    // Any Employee can see the shared calendar
    [Authorize]
    [HttpGet(nameof(GetSharedCalendar))]
    public async Task<ActionResult<List<SharedCalendarEmployeeDto>>> GetSharedCalendar(DateOnly start, DateOnly end)
    {
        return Ok(await service.GetCalendar(start, end));
    }
}