using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs.TransactionDTOs;
using Service.HolidayServices;

namespace API.Controllers;

[ApiController]
public class TransactionController (TransactionService service) : ControllerBase
{
    [Authorize]
    [HttpGet(nameof(GetTransactionHistory))]
    public async Task<ActionResult<List<HolidayTransactionDto>>> GetTransactionHistory(Guid accountId)
    {
        return await service.GetHistory(accountId);
    }

    [Authorize(Policy = "Admin")]
    [HttpPost(nameof(CreateTransaction))]
    public async Task<ActionResult<HolidayTransactionDto>> CreateTransaction([FromBody]CreateHolidayTransactionDto dto)
    {
        return Ok(await service.Create(dto));
    }
    
    [Authorize]
    [HttpGet(nameof(GetTransactionHistoryForUser))]
    public async Task<ActionResult<List<HolidayTransactionOverviewDto>>>
        GetTransactionHistoryForUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        return Ok(await service.GetUserHistory(userId));
    }
}