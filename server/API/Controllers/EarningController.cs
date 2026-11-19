using Microsoft.AspNetCore.Mvc;
using Service.DTOs.HolidayDTOs;
using Service.HolidayServices.Earnings;

namespace API.Controllers;

public class EarningController(EarningService service) : ControllerBase
{
    [HttpGet(nameof(GetEarningsForUser))]
    public async Task<UserEarningsDto> GetEarningsForUser(string userId)
    {
        return await service.GetEarningsByUser(userId);
    }
}