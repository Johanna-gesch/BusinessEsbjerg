using System.Security.Claims;
using Infra.Entities.HolidayTracking.Requests;
using Infra.Entities.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs.RequestDTOs;
using Service.HolidayServices;

namespace API.Controllers;

public class RequestController(RequestService requestService) : ControllerBase
{
    [HttpGet(nameof(GetAllRequests))]
    public async Task<List<HolidayRequest>> GetAllRequests()
    {
        return await requestService.GetAllRequests();
    }

    [HttpGet(nameof(GetRequestsForUser))]
    public async Task<ActionResult<List<HolidayRequest>>> GetRequestsForUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var requests = await requestService.GetRequestsForUser(userId);

        return Ok(requests);
    }
    
    [HttpPost(nameof(CreateRequest))]
    public async Task<IActionResult> CreateRequest([FromBody] CreateRequestDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        await requestService.CreateRequestAsync(dto, userId);

        return Ok();
    }

    [Authorize(Roles = nameof(UserRole.Accountant))]
    [HttpPut(nameof(UpdateRequestStatus))]
    public async Task<IActionResult> UpdateRequestStatus(Guid requestId, HolidayRequestStatus status)
    {
        await requestService.UpdateRequestStatusAsync(requestId, status);

        return NoContent();
    }

    [HttpPut(nameof(CancelRequest))]
    public async Task<IActionResult> CancelRequest(Guid requestId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        await requestService.CancelRequestAsync(requestId, userId);

        return NoContent();
    }
}