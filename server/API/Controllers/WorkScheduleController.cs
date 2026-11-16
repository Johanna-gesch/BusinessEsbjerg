using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs.WorkScheduleDTOs;
using Service.HolidayServices;

namespace API.Controllers;

public class WorkScheduleController (WorkScheduleService service) : ControllerBase
{

    [Authorize]
    [HttpGet(nameof(GetEffectiveWorkSchedule))]
    public async Task<ActionResult<WorkScheduleDto>> GetEffectiveWorkSchedule(string userId, DateOnly date)
    {
        var schedule = await service.GetEffective(userId, date);
        return Ok(schedule);
    }
    
}