using Infra;
using Infra.Entities.HolidayTracking.Requests;
using Infra.Entities.HolidayTracking.Transactions;
using Infra.Repositories.Interfaces;
using Service.DTOs.RequestDTOs;

namespace Service.HolidayServices;

public class RequestService(
    IRequestRepo repo,
    WorkScheduleService workScheduleService,
    HolidayBalanceService holidayBalanceService,
    CalendarDayService calendarDayService,
    ITransactionRepo transactionRepo,
    IUnitOfWork unitOfWork)
{
    public async Task<List<HolidayRequest>> GetAllRequests()
    {
        return await repo.GetAll();
    }

    public async Task<List<HolidayRequest>> GetRequestsForUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ValidationException("User id is required.");

        return await repo.GetByUserId(userId);
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

        if (string.IsNullOrWhiteSpace(userId))
            throw new ValidationException("User id is required.");
        
        if (await IsOverlapAsync(userId, dto.StartDate, dto.EndDate))
            throw new ValidationException(
                "There is overlap with another one of your requests.");
        

        var request = new HolidayRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
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

        if (status == HolidayRequestStatus.Rejected)
        {
            request.Status = HolidayRequestStatus.Rejected;
            await repo.Update(request);
            return;
        }

        await ApproveRequestAsync(request);
    }
    
    private async Task ApproveRequestAsync(HolidayRequest request)
    {
        var calculationDate = DateOnly.FromDateTime(DateTime.UtcNow);
        
        var publicHolidays = await calendarDayService.GetHolidayDates(
            request.StartDate,
            request.EndDate);
        
        var requiredHours = await workScheduleService.CalculateRequiredHours(
            request.UserId,
            request.StartDate,
            request.EndDate,
            publicHolidays);

        var balance = await holidayBalanceService.GetUserBalance(
            request.UserId,
            DateOnly.FromDateTime(DateTime.UtcNow));

        var debtHours = Math.Max(
            0,
            requiredHours - balance.TotalHours);

        if (!await holidayBalanceService.IsDebtAllowed(
                request.UserId, 
                debtHours, 
                request.StartDate))
        {
            throw new ValidationException(
                "The request exceeds the permitted holiday debt.");
        }

        // August 31 projected balance validation goes here.
        
        var hoursToAllocate = Math.Min(requiredHours, balance.TotalHours);

        var transactions = hoursToAllocate > 0
            ? await holidayBalanceService.AllocateHours(
                request.UserId,
                hoursToAllocate,
                calculationDate)
            : new List<HolidayTransaction>();

        await unitOfWork.BeginTransactionAsync();

        try
        {
            foreach (var transaction in transactions)
            {
                await transactionRepo.Insert(transaction);
            }

            request.Status = HolidayRequestStatus.Approved;
            await repo.Update(request);

            await unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
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
}