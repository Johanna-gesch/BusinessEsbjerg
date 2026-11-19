using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs.WorkScheduleDTOs;
using Service.HolidayServices;

namespace API.Controllers;

[ApiController]
public class WorkScheduleController (WorkScheduleService service) : ControllerBase
{

    [Authorize]
    [HttpGet(nameof(GetEffectiveWorkSchedule))]
    public async Task<ActionResult<WorkScheduleDto>> GetEffectiveWorkSchedule(string userId, DateOnly date)
    {
        var schedule = await service.GetEffective(userId, date);
        return Ok(schedule);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost(nameof(CreateWorkSchedule))]
    public async Task<ActionResult<WorkScheduleDto>> CreateWorkSchedule([FromBody] CreateWorkScheduleDto dto)
    {
        return Ok(await service.Create(dto));
    }
    
}