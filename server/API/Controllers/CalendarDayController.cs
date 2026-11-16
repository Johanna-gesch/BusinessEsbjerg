using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs.CalendarDayDTOs;
using Service.HolidayServices;

namespace API.Controllers;

[ApiController]
public class CalendarDayController (CalendarDayService service) : ControllerBase
{
    [Authorize(Policy = "Admin")]
    [HttpPost(nameof(CreateCalendarDay))]
    
    public async Task<ActionResult<CalendarDayDTO>> CreateCalendarDay ([FromBody] CreateCalendarDayDTO dto)
    {
        var created = await service.Create(dto);
        return Ok(created);

    }
    
    
}