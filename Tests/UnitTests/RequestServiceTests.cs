using Infra;
using Infra.Entities;
using Infra.Entities.HolidayTracking.Accounts;
using Infra.Entities.HolidayTracking.Earnings;
using Infra.Entities.HolidayTracking.Requests;
using Infra.Entities.HolidayTracking.WorkSchedule;
using Service.DTOs.RequestDTOs;
using Service.HolidayServices;
using Service.HolidayServices.Earnings;
using Tests.UnitTests.Stubs;

namespace Tests.UnitTests;

public class RequestServiceTests
{
    private readonly RequestService _requestService;

    private readonly RequestRepoStub _repoStub = new();
    private readonly WorkScheduleRepoStub _scheduleRepoStub = new();
    private readonly UserRepoStub _userRepoStub = new();
    private readonly CalendarDayRepoStub _calendarDayRepoStub = new();
    private readonly EarningRepoStub _earningRepoStub = new();
    private readonly AccountRepoStub _accountRepoStub = new();
    private readonly TransactionRepoStub _transactionRepoStub = new();
    private readonly UnitOfWorkStub _unitOfWorkStub = new();

    private readonly HolidayBalanceService _balanceService;

    public RequestServiceTests()
    {
        var workScheduleService = new WorkScheduleService(
            _scheduleRepoStub,
            _userRepoStub);

        var calendarDayService = new CalendarDayService(
            _calendarDayRepoStub);

        var earningService = new EarningService(
            new EarningCalculator(),
            _earningRepoStub,
            _accountRepoStub,
            _userRepoStub,
            _scheduleRepoStub);

        _balanceService = new HolidayBalanceService(
            _earningRepoStub,
            _transactionRepoStub,
            _accountRepoStub,
            workScheduleService,
            earningService);

        _requestService = new RequestService(
            _repoStub,
            workScheduleService,
            _balanceService,
            calendarDayService,
            _transactionRepoStub,
            _unitOfWorkStub);
    }

    // *---* CREATE TESTS *---* //

    #region Create Tests

    [Fact]
    public async Task Insert_ValidRequest_RequestIsCreated()
    {
        // Arrange
        var userId = "user123";

        var dto = new CreateRequestDto
        {
            StartDate = new DateOnly(2027, 8, 31),
            EndDate = new DateOnly(2027, 9, 7),
        };

        // Act
        await _requestService.CreateRequestAsync(dto, userId);

        // Assert
        var requests = await _repoStub.GetAll();

        var createdRequest = Assert.Single(requests);

        Assert.Equal(userId, createdRequest.UserId);
        Assert.Equal(dto.StartDate, createdRequest.StartDate);
        Assert.Equal(dto.EndDate, createdRequest.EndDate);
        Assert.Equal(HolidayRequestStatus.Pending, createdRequest.Status);
        Assert.NotEqual(Guid.Empty, createdRequest.Id);
    }

    [Fact]
    public async Task Insert_EndDateBeforeStartDate_ThrowsException()
    {
        // Arrange
        var userId = "user123";

        var dto = new CreateRequestDto
        {
            StartDate = new DateOnly(2027, 9, 7),
            EndDate = new DateOnly(2027, 8, 31),
        };

        // Act and assert
        await Assert.ThrowsAsync<ValidationException>(() => _requestService.CreateRequestAsync(dto, userId));
    }

    [Fact]
    public async Task Insert_EmptyUserId_ThrowsException()
    {
        // Arrange
        var dto = new CreateRequestDto
        {
            StartDate = new DateOnly(2027, 8, 31),
            EndDate = new DateOnly(2027, 9, 7)
        };

        // Act and Assert
        await Assert.ThrowsAsync<ValidationException>(() => _requestService.CreateRequestAsync(dto, ""));
    }

    #endregion

    // *---* READ TESTS *---* //

    #region Read Tests

    [Fact]
    public async Task GetById_ExistingRequest_ReturnsRequest()
    {
        // Arrange
        var request = CreateRequest();

        await _repoStub.Insert(request);

        // Act 
        var result = await _requestService.GetById(request.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Id, result.Id);
        Assert.Equal(request.UserId, result.UserId);
        Assert.Equal(request.StartDate, result.StartDate);
        Assert.Equal(request.EndDate, result.EndDate);
    }

    [Fact]
    public async Task GetById_EmptyId_ThrowsValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() => _requestService.GetById(Guid.Empty));
    }

    #endregion


    // *---* UPDATE TESTS *---* //

    #region Update Tests

    [Fact(Skip = "Approval test needs complete schedule, calendar and earning data.")]
    public async Task Update_ValidRequest_StatusIsApproved()
    {
        // Arrange
        var request = CreateRequest();
        await _repoStub.Insert(request);

        // Act
        await _requestService.UpdateRequestStatusAsync(
            request.Id,
            HolidayRequestStatus.Approved);

        // Assert
        var updatedRequest = await _repoStub.GetById(request.Id);

        Assert.NotNull(updatedRequest);
        Assert.Equal(
            HolidayRequestStatus.Approved,
            updatedRequest.Status);
        Assert.True(_unitOfWorkStub.TransactionStarted);
        Assert.True(_unitOfWorkStub.TransactionCommitted);
    }
    
    /**
     * This test does the following:
     * Calculates the required hours
     * Checks available balance
     * Checks debt
     * Allocates hours
     * Approves request
     */
    [Fact]
    public async Task ApproveRequest_WithSufficientBalance_ApprovesRequest()
    {
        // Arrange
        var userId = "user123";

        var startDate = new DateOnly(2027, 9, 6); // Monday
        var endDate = new DateOnly(2027, 9, 10); // Friday

        var request = CreateRequest();
        request.UserId = userId;
        request.StartDate = startDate;
        request.EndDate = endDate;

        await _repoStub.Insert(request);

        // Add a 37-hour work schedule.
        _scheduleRepoStub.Current.Add(new WorkSchedule
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EffectiveFrom = new DateOnly(2026, 1, 1),
            MondayHours = 7.4m,
            TuesdayHours = 7.4m,
            WednesdayHours = 7.4m,
            ThursdayHours = 7.4m,
            FridayHours = 7.4m
        });

        _scheduleRepoStub.History.Add(new WorkScheduleHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EffectiveFrom = new DateOnly(2026, 1, 1),
            MondayHours = 7.4m,
            TuesdayHours = 7.4m,
            WednesdayHours = 7.4m,
            ThursdayHours = 7.4m,
            FridayHours = 7.4m
        });

        // Add both holiday accounts.
        var week1_5Account = new HolidayAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = HolidayType.Week1_5
        };

        var week6Account = new HolidayAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = HolidayType.Week6
        };

        await _accountRepoStub.InsertAsync(week1_5Account);
        await _accountRepoStub.InsertAsync(week6Account);

        // Give the Week1_5 account 40 available hours.
        await _earningRepoStub.SaveEarningsTransaction(
            new List<HolidayEarning>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    HolidayAccountId = week1_5Account.Id,
                    EarnedDate = new DateOnly(2026, 1, 1),
                    EarnedHours = 40m,
                    EarnedDays = 0m,
                    ExpiryDate = new DateOnly(2099, 12, 31)
                }
            });

        // Act
        await _requestService.UpdateRequestStatusAsync(
            request.Id,
            HolidayRequestStatus.Approved);

        // Assert
        var updatedRequest = await _repoStub.GetById(request.Id);

        Assert.NotNull(updatedRequest);
        Assert.Equal(
            HolidayRequestStatus.Approved,
            updatedRequest.Status);

        Assert.NotEmpty(_transactionRepoStub.Transactions);

        Assert.True(_unitOfWorkStub.TransactionStarted);
        Assert.True(_unitOfWorkStub.TransactionCommitted);
        Assert.False(_unitOfWorkStub.TransactionRolledBack);
    }
    
    /**
     * This test ensures that the debt check happens before the transaction begins.
     * The request isn't approved, nor is it cancelled.
     * It remains pending.
     */
    [Fact]
    public async Task ApproveRequest_InsufficientBalanceAndDebtNotAllowed_ThrowsValidationException()
    {
        // Arrange
        var userId = "user123";

        var startDate = new DateOnly(2027, 9, 6); // Monday
        var endDate = new DateOnly(2027, 9, 17); // Friday, following week

        var request = CreateRequest();
        request.UserId = userId;
        request.StartDate = startDate;
        request.EndDate = endDate;

        await _repoStub.Insert(request);

        // Add a 37-hour work schedule.
        _scheduleRepoStub.Current.Add(new WorkSchedule
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EffectiveFrom = new DateOnly(2026, 1, 1),
            MondayHours = 7.4m,
            TuesdayHours = 7.4m,
            WednesdayHours = 7.4m,
            ThursdayHours = 7.4m,
            FridayHours = 7.4m
        });

        _scheduleRepoStub.History.Add(new WorkScheduleHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EffectiveFrom = new DateOnly(2026, 1, 1),
            MondayHours = 7.4m,
            TuesdayHours = 7.4m,
            WednesdayHours = 7.4m,
            ThursdayHours = 7.4m,
            FridayHours = 7.4m
        });

        // Add both holiday accounts.
        var week1_5Account = new HolidayAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = HolidayType.Week1_5
        };

        var week6Account = new HolidayAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = HolidayType.Week6
        };

        await _accountRepoStub.InsertAsync(week1_5Account);
        await _accountRepoStub.InsertAsync(week6Account);

        // Give the user only 10 available hours.
        await _earningRepoStub.SaveEarningsTransaction(
            new List<HolidayEarning>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    HolidayAccountId = week1_5Account.Id,
                    EarnedDate = new DateOnly(2026, 1, 1),
                    EarnedHours = 10m,
                    EarnedDays = 0m,
                    ExpiryDate = new DateOnly(2099, 12, 31)
                }
            });

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _requestService.UpdateRequestStatusAsync(
            request.Id,
            HolidayRequestStatus.Approved));

        // The request should still be pending.
        var unchangedRequest = await _repoStub.GetById(request.Id);

        Assert.NotNull(unchangedRequest);
        Assert.Equal(
            HolidayRequestStatus.Pending,
            unchangedRequest.Status);

        // No transactions should have been created.
        Assert.Empty(_transactionRepoStub.Transactions);

        // No database transaction should have been started.
        Assert.False(_unitOfWorkStub.TransactionStarted);
        Assert.False(_unitOfWorkStub.TransactionCommitted);
        Assert.False(_unitOfWorkStub.TransactionRolledBack);
    }
    
    [Fact]
    public async Task ApproveRequest_ExistingApprovedRequestMakesBalanceNegative_ThrowsValidationException()
    {
        // Arrange
        var userId = "user123";
        var year = DateTime.Today.Year + 1;

        await AddApprovalTestData(userId);

        var existingRequest = new HolidayRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StartDate = new DateOnly(year, 9, 1),
            EndDate = new DateOnly(year + 1, 8, 29),
            CreatedAt = DateTime.UtcNow,
            Status = HolidayRequestStatus.Approved
        };

        await _repoStub.Insert(existingRequest);

        var currentRequest = new HolidayRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StartDate = new DateOnly(year + 1, 8, 30),
            EndDate = new DateOnly(year + 1, 8, 30),
            CreatedAt = DateTime.UtcNow,
            Status = HolidayRequestStatus.Pending
        };

        await _repoStub.Insert(currentRequest);

        // Act
        await Assert.ThrowsAsync<ValidationException>(() =>
            _requestService.UpdateRequestStatusAsync(
                currentRequest.Id,
                HolidayRequestStatus.Approved));

        // Assert
        var unchangedRequest = await _repoStub.GetById(currentRequest.Id);

        Assert.NotNull(unchangedRequest);
        Assert.Equal(HolidayRequestStatus.Pending, unchangedRequest.Status);

        Assert.Empty(_transactionRepoStub.Transactions);
        Assert.False(_unitOfWorkStub.TransactionStarted);
        Assert.False(_unitOfWorkStub.TransactionCommitted);
    }
    
    [Fact]
    public async Task ApproveRequest_NonApprovedRequestsAreIgnored_ApprovesRequest()
    {
        // Arrange
        var userId = "user123";
        var year = DateTime.Today.Year + 1;

        await AddApprovalTestData(userId);

        var statuses = new[]
        {
            HolidayRequestStatus.Pending,
            HolidayRequestStatus.Rejected,
            HolidayRequestStatus.Cancelled
        };

        foreach (var status in statuses)
        {
            await _repoStub.Insert(new HolidayRequest
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StartDate = new DateOnly(year, 9, 1),
                EndDate = new DateOnly(year + 1, 8, 29),
                CreatedAt = DateTime.UtcNow,
                Status = status
            });
        }

        var currentRequest = new HolidayRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StartDate = new DateOnly(year + 1, 8, 30),
            EndDate = new DateOnly(year + 1, 8, 30),
            CreatedAt = DateTime.UtcNow,
            Status = HolidayRequestStatus.Pending
        };

        await _repoStub.Insert(currentRequest);

        // Act
        await _requestService.UpdateRequestStatusAsync(
            currentRequest.Id,
            HolidayRequestStatus.Approved);

        // Assert
        var updatedRequest = await _repoStub.GetById(currentRequest.Id);

        Assert.NotNull(updatedRequest);
        Assert.Equal(HolidayRequestStatus.Approved, updatedRequest.Status);

        Assert.True(_unitOfWorkStub.TransactionStarted);
        Assert.True(_unitOfWorkStub.TransactionCommitted);
        Assert.False(_unitOfWorkStub.TransactionRolledBack);
    }
    
    [Fact]
    public async Task ApproveRequest_CrossesAugust31_ChecksBothHolidayYears()
    {
        // Arrange
        var userId = "user123";
        var year = DateTime.Today.Year + 1;

        await AddApprovalTestData(userId);

        var request = new HolidayRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StartDate = new DateOnly(year, 8, 30),
            EndDate = new DateOnly(year, 9, 3),
            CreatedAt = DateTime.UtcNow,
            Status = HolidayRequestStatus.Pending
        };

        await _repoStub.Insert(request);

        // Act
        await _requestService.UpdateRequestStatusAsync(
            request.Id,
            HolidayRequestStatus.Approved);

        // Assert
        var updatedRequest = await _repoStub.GetById(request.Id);

        Assert.NotNull(updatedRequest);
        Assert.Equal(HolidayRequestStatus.Approved, updatedRequest.Status);

        Assert.True(_unitOfWorkStub.TransactionStarted);
        Assert.True(_unitOfWorkStub.TransactionCommitted);
    }

    [Fact]
    public async Task Update_ValidRequest_StatusIsRejected()
    {
        // Arrange
        var request = CreateRequest();
        await _repoStub.Insert(request);

        // Act
        await _requestService.UpdateRequestStatusAsync(
            request.Id,
            HolidayRequestStatus.Rejected);

        // Assert
        var updatedRequest = await _repoStub.GetById(request.Id);

        Assert.NotNull(updatedRequest);
        Assert.Equal(
            HolidayRequestStatus.Rejected,
            updatedRequest.Status);
    }

    [Fact]
    public async Task Update_AlreadyApprovedRequest_ThrowsValidationException()
    {
        // Arrange
        var request = CreateRequest();
        request.Status = HolidayRequestStatus.Approved;

        await _repoStub.Insert(request);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _requestService.UpdateRequestStatusAsync(
            request.Id,
            HolidayRequestStatus.Rejected));
    }

    [Fact]
    public async Task CancelRequest_OwnRequest_StatusIsCancelled()
    {
        // Arrange
        var userId = "user123";
        var request = CreateRequest();

        request.UserId = userId;
        await _repoStub.Insert(request);

        // Act
        await _requestService.CancelRequestAsync(
            request.Id,
            userId);

        // Assert
        var updatedRequest = await _repoStub.GetById(request.Id);

        Assert.NotNull(updatedRequest);
        Assert.Equal(
            HolidayRequestStatus.Cancelled,
            updatedRequest.Status);
    }

    [Fact]
    public async Task CancelRequest_OtherUsersRequest_ThrowsValidationException()
    {
        // Arrange
        var request = CreateRequest();
        request.UserId = "user123";

        await _repoStub.Insert(request);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _requestService.CancelRequestAsync(
            request.Id,
            "user456"));
    }

    [Fact]
    public async Task CancelRequest_ApprovedRequest_ThrowsValidationException()
    {
        // Arrange
        var userId = "user123";

        var request = CreateRequest();
        request.UserId = userId;
        request.Status = HolidayRequestStatus.Approved;

        await _repoStub.Insert(request);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _requestService.CancelRequestAsync(
            request.Id,
            userId));
    }

    #endregion

    private static HolidayRequest CreateRequest()
    {
        return new HolidayRequest
        {
            Id = Guid.NewGuid(),
            UserId = "user123",
            StartDate = new DateOnly(2027, 8, 31),
            EndDate = new DateOnly(2027, 9, 7),
            CreatedAt = DateTime.UtcNow,
            Status = HolidayRequestStatus.Pending
        };
    }
    
    private async Task AddApprovalTestData(string userId)
    {
        var schedule = new WorkSchedule
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EffectiveFrom = new DateOnly(DateTime.Today.Year - 1, 1, 1),
            MondayHours = 7.4m,
            TuesdayHours = 7.4m,
            WednesdayHours = 7.4m,
            ThursdayHours = 7.4m,
            FridayHours = 7.4m
        };

        _scheduleRepoStub.Current.Add(schedule);

        _scheduleRepoStub.History.Add(new WorkScheduleHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EffectiveFrom = schedule.EffectiveFrom,
            MondayHours = 7.4m,
            TuesdayHours = 7.4m,
            WednesdayHours = 7.4m,
            ThursdayHours = 7.4m,
            FridayHours = 7.4m
        });

        var week1_5Account = new HolidayAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = HolidayType.Week1_5
        };

        var week6Account = new HolidayAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = HolidayType.Week6
        };

        await _accountRepoStub.InsertAsync(week1_5Account);
        await _accountRepoStub.InsertAsync(week6Account);

        await _earningRepoStub.SaveEarningsTransaction(
            new List<HolidayEarning>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    HolidayAccountId = week1_5Account.Id,
                    EarnedDate = new DateOnly(DateTime.Today.Year - 1, 1, 1),
                    EarnedHours = 40m,
                    EarnedDays = 0m,
                    ExpiryDate = new DateOnly(2099, 12, 31)
                }
            });
    }
}