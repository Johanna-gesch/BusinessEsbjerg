using Infra.Entities.HolidayTracking.Salary;
using Microsoft.AspNetCore.Mvc;
using Service.HolidayServices;

namespace API.Controllers;

[ApiController]
public class SalaryController(SalaryHistoryService service) : ControllerBase
{
    [HttpGet(nameof(GetSalaryHistoryForUser))]
    public async Task<List<SalaryHistory>> GetSalaryHistoryForUser(string userId) {
        
        return await service.GetSalaryHistoryForUser(userId);
    }
    
}