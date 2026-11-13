using Infra.Entities.HolidayTracking.Accounts;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs.HolidayDTOs;
using Service.HolidayServices;

namespace API.Controllers;

[ApiController]
public class AccountController(AccountService service) : ControllerBase
{
   /*
    [HttpPost(nameof(CreateAccount))]
    public async Task<ActionResult<HolidayAccount>> CreateAccount(
        [FromBody] CreateHolidayAccountDto dto)
    {
        var account = await service.CreateAccountAsync(dto);

        return CreatedAtAction(
            nameof(GetAccountById),
            new { id = account.Id },
            account);
    }*/

    [HttpGet(nameof(GetAccountById))]
    public async Task<ActionResult<HolidayAccount>> GetAccountById(Guid id)
    {
        var account = await service.GetAccountById(id);
        return Ok(account);
    }

    [HttpGet(nameof(GetAccountsForUser))]
    public async Task<ActionResult<ICollection<HolidayAccount>>> GetAccountsForUser(string userId)
    {
        var accounts = await service.GetAccountsForUserAsync(userId);

        return Ok(accounts);
    }
}