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
    IUserRepo userRepo,
    IUnitOfWork unitOfWork)
{
    private const int MaxRejectionReasonLength = 255;
    
    public async Task<List<HolidayRequestDto>> GetAllRequests()
    {
        var requests = await repo.GetAll();

        // One query for all users, then look the name up in memory
        var usernames = (await userRepo.GetAll())
            .ToDictionary(u => u.UserId, u => u.Username);

        return requests
            .OrderBy(r => r.Status == HolidayRequestStatus.Pending ? 0 : 1)
            .ThenByDescending(r => r.CreatedAt)
            .Select(r => ToDto(r, usernames.GetValueOrDefault(r.UserId, "Unknown")))
            .ToList();
    }

    public async Task<List<HolidayRequestDto>> GetRequestsForUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ValidationException("User id is required.");

        var user = await userRepo.GetById(userId);
        var requests = await repo.GetByUserId(userId);

        return requests.Select(r => ToDto(r, user?.Username ?? "Unknown")).ToList();
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
    
    private static HolidayRequestDto ToDto(HolidayRequest request, string username) => new()
    {
        Id = request.Id,
        UserName = username,
        CreatedAt = request.CreatedAt,
        StartDate = request.StartDate,
        EndDate = request.EndDate,
        Status = request.Status,
        RejectionReason = request.RejectionReason,
        CancellationReason = request.CancellationReason
    };

    private async Task<bool> IsOverlapAsync(string userId, DateOnly startDate, DateOnly endDate)
    {
        var existingRequests = await repo.GetByUserId(userId);

        return existingRequests.Any(request =>
            request.Status != HolidayRequestStatus.Rejected &&
            request.Status != HolidayRequestStatus.Cancelled &&
            startDate <= request.EndDate &&
            endDate >= request.StartDate);
    }

    public async Task UpdateRequestStatusAsync(
        Guid requestId,
        HolidayRequestStatus status,
        string? rejectionReason = null)
    {
        if (status != HolidayRequestStatus.Approved &&
            status != HolidayRequestStatus.Rejected)
            throw new ValidationException("Admins can only approve or reject requests.");

        var request = await GetById(requestId);

        if (request.Status != HolidayRequestStatus.Pending)
            throw new ValidationException("Only pending requests can be updated.");

        if (status == HolidayRequestStatus.Rejected)
        {
            // The employee must be told why, so a reason is required
            var reason = rejectionReason?.Trim();

            if (string.IsNullOrEmpty(reason))
                throw new ValidationException("A rejection reason is required.");

            if (reason.Length > MaxRejectionReasonLength)
                throw new ValidationException(
                    $"The rejection reason can be at most {MaxRejectionReasonLength} characters.");

            request.Status = HolidayRequestStatus.Rejected;
            request.RejectionReason = reason; // saved in the database
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

        await ValidateAugust31Balances(request);

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
                // Now all allocations belonging to the same request, share the same 
                // HolidayRequestId.
                transaction.HolidayRequestId = request.Id;
                transaction.Description =
                    $"Holiday request ({request.StartDate:dd-MM-yyyy} to {request.EndDate:dd-MM-yyyy})";
                
                Console.WriteLine($"Request ID: {request.Id}");
                Console.WriteLine($"Transaction ID: {transaction.Id}");
                Console.WriteLine($"Holiday Request ID on transaction: {transaction.HolidayRequestId}");
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

    private async Task ValidateAugust31Balances(
        HolidayRequest currentRequest)
    {
        var requests = await repo.GetByUserId(currentRequest.UserId);

        // Include approved requests, but exclude the current request
        // so that we can add it exactly once below.
        var qualifyingRequests = requests
            .Where(r =>
                r.Id != currentRequest.Id &&
                r.Status == HolidayRequestStatus.Approved)
            .ToList();

        // The request currently being reviewed must also count.
        qualifyingRequests.Add(currentRequest);

        // Determine the first and last holiday years affected.
        // A holiday year runs from September 1 to August 31.
        int firstCheckpointYear =
            currentRequest.StartDate.Month >= 9
                ? currentRequest.StartDate.Year + 1
                : currentRequest.StartDate.Year;

        int lastCheckpointYear =
            currentRequest.EndDate.Month >= 9
                ? currentRequest.EndDate.Year + 1
                : currentRequest.EndDate.Year;

        for (int year = firstCheckpointYear;
             year <= lastCheckpointYear;
             year++)
        {
            var checkpoint = new DateOnly(year, 8, 31);

            var holidayYearStart = new DateOnly(year - 1, 9, 1);

            // Calculate the projected balance before holiday usage.
            var projectedBalance =
                await holidayBalanceService.GetUserProjectedBalance(
                    currentRequest.UserId,
                    checkpoint
                );

            decimal approvedHolidayHours = 0;

            foreach (var request in qualifyingRequests)
            {
                // Find the part of this request that belongs to
                // the holiday year being checked.
                var periodStart = request.StartDate < holidayYearStart
                    ? holidayYearStart
                    : request.StartDate;

                var periodEnd = request.EndDate > checkpoint
                    ? checkpoint
                    : request.EndDate;

                // This request does not overlap the holiday year.
                if (periodStart > periodEnd)
                    continue;

                var publicHolidays =
                    await calendarDayService.GetHolidayDates(
                        periodStart,
                        periodEnd
                    );

                var requiredHours =
                    await workScheduleService.CalculateRequiredHours(
                        request.UserId,
                        periodStart,
                        periodEnd,
                        publicHolidays
                    );

                approvedHolidayHours += requiredHours;
            }

            var remainingBalance =
                projectedBalance - approvedHolidayHours;

            if (remainingBalance < 0)
            {
                throw new ValidationException(
                    $"Approving this request would result in a negative " +
                    $"holiday balance on August 31, {year}. " +
                    $"Projected balance: {remainingBalance:F2} hours."
                );
            }
        }
    }
}