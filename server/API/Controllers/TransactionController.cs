using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs.TransactionDTOs;
using Service.HolidayServices;

namespace API.Controllers;

public class TransactionController (TransactionService service) : ControllerBase
{
    [Authorize]
    [HttpGet(nameof(GetTransactionHistory))]
    public async Task<ActionResult<List<HolidayTransactionDto>>> GetTransactionHistory(Guid accountId)
    {
        return await service.GetHistory(accountId);
    }
    
}