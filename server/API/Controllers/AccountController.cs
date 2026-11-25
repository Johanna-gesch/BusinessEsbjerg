using System.Security.Claims;
using Infra.Entities.HolidayTracking.Accounts;
using Infra.Entities.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs;
using Service.DTOs.HolidayDTOs;
using Service.HolidayServices;

namespace API.Controllers;

[ApiController]
public class AccountController(AccountService service, AccountOverviewService overviewService, HolidayBalanceService holidayBalanceService) : ControllerBase
{
    [Authorize]
    [HttpGet(nameof(GetAccountById))]
    public async Task<ActionResult<HolidayAccount>> GetAccountById(Guid id)
    {
        var account = await service.GetAccountById(id);
        return Ok(account);
    }

    [Authorize]
    [HttpGet(nameof(GetAccountsForUser))]
    public async Task<ActionResult<ICollection<HolidayAccount>>> GetAccountsForUser(string userId)
    {
        var accounts = await service.GetAccountsForUserAsync(userId);

        return Ok(accounts);
    }

    [Authorize(Roles = "Accountant")]
    [HttpGet(nameof(GetAccountOverview))]
    public async Task<ActionResult<List<EmployeebalanceDto>>> GetAccountOverview()
    {
        var overview = await overviewService.GetOverview(DateOnly.FromDateTime(DateTime.Today));
        return Ok(overview);
    }
    
    [Authorize]
    [HttpGet(nameof(GetAvailableBalance))]
    public async Task<ActionResult<decimal>> GetAvailableBalance(
        Guid accountId,
        DateOnly date)
    {
        var balance = await holidayBalanceService.GetAvailableBalance(accountId, date);
        return Ok(balance);
    }
}