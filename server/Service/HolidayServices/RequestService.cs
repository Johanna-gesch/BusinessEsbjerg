using Infra;
using Infra.Entities.HolidayTracking.Requests;
using Infra.Repositories.Interfaces;
using Service.DTOs.RequestDTOs;

namespace Service.HolidayServices;

public class RequestService(
    IRequestRepo repo,
    IAccountRepo accountRepo)
{
    public async Task<List<HolidayRequest>> GetAllRequests()
    {
        return await repo.GetAll();
    }
    
    public async Task<HolidayRequest> GetById(Guid id)
    {
        if (id == Guid.Empty)
            throw new ValidationException("Request Id can't be empty.");

        return await repo.GetById(id) ??
               throw new NotFoundException($"Request with this Id {id} is not found.");
    }
    
    public async Task CreateRequestAsync(CreateRequestDto dto, string userId)
    {
        if (dto.StartDate > dto.EndDate)
            throw new ValidationException(
                "The holiday must start before or on the end date.");

        await ValidateAccountOwnershipAsync(dto.HolidayAccountId, userId);

        if (await IsOverlapAsync(userId, dto.StartDate, dto.EndDate))
            throw new ValidationException(
                "There is overlap with another one of your requests.");
        

        var request = new HolidayRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            HolidayAccountId = dto.HolidayAccountId,
            CreatedAt = DateTime.UtcNow,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = HolidayRequestStatus.Pending
        };

        await repo.Insert(request);
    }

    private async Task<bool> IsOverlapAsync(string userId, DateOnly startDate, DateOnly endDate)
    {
        var existingRequests = await repo.GetByUserId(userId);

        return existingRequests.Any(request =>
            request.Status != HolidayRequestStatus.Rejected &&
            request.Status != HolidayRequestStatus.Cancelled &&
            startDate <= request.EndDate &&
            endDate >= request.StartDate);
    }

    public async Task UpdateRequestStatusAsync(Guid requestId, HolidayRequestStatus status)
    {
        if (status != HolidayRequestStatus.Approved &&
            status != HolidayRequestStatus.Rejected)
            throw new ValidationException("Admins can only approve or reject requests.");
        

        var request = await GetById(requestId);

        if (request.Status != HolidayRequestStatus.Pending)
            throw new ValidationException("Only pending requests can be updated.");

        request.Status = status;

        await repo.Update(request);
    }

    public async Task CancelRequestAsync(Guid requestId, string userId)
    {
        var request = await GetById(requestId);

        if (request.UserId != userId)
            throw new ValidationException("You can only cancel your own requests.");

        if (request.Status != HolidayRequestStatus.Pending)
            throw new ValidationException("Only pending requests can be cancelled.");

        request.Status = HolidayRequestStatus.Cancelled;

        await repo.Update(request);
    }

    private async Task ValidateAccountOwnershipAsync(Guid accountId, string userId)
    {
        var account = await accountRepo.GetByIdAsync(accountId);

        if (account == null)
        {
            throw new ValidationException(
                "The holiday account doesn't exist.");
        }

        if (account.UserId != userId)
        {
            throw new ValidationException(
                "You can't create a request for another user's holiday account.");
        }
    }
}