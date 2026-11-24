using Infra;
using Infra.Entities;
using Infra.Entities.HolidayTracking.Accounts;
using Infra.Entities.HolidayTracking.Requests;
using Service.DTOs.RequestDTOs;
using Service.HolidayServices;
using Tests.UnitTests.Stubs;

namespace Tests.UnitTests;

public class RequestServiceTests
{
    private readonly RequestService _requestService;
    private readonly RequestRepoStub _repoStub = new();
    private readonly AccountRepoStub _accountRepoStub = new();

    public RequestServiceTests()
    {
        _requestService = new RequestService(_repoStub, _accountRepoStub);
    }

    // *---* CREATE TESTS *---* //

    #region Create Tests

    [Fact]
    public async Task Insert_ValidRequest_RequestIsCreated()
    {
        // Arrange
        var userId = "user123";

        var account = new HolidayAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = HolidayType.Week1_5
        };

        await _accountRepoStub.InsertAsync(account);

        var dto = new CreateRequestDto
        {
            StartDate = new DateOnly(2027, 8, 31),
            EndDate = new DateOnly(2027, 9, 7),
            HolidayAccountId = account.Id
        };

        // Act
        await _requestService.CreateRequestAsync(dto, userId);

        // Assert
        var requests = await _repoStub.GetAll();

        var createdRequest = requests.First();

        Assert.Equal(userId, createdRequest.UserId);
        Assert.Equal(account.Id, createdRequest.HolidayAccountId);
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

        var account = new HolidayAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = HolidayType.Week1_5
        };

        await _accountRepoStub.InsertAsync(account);

        var dto = new CreateRequestDto
        {
            StartDate = new DateOnly(2027, 9, 7),
            EndDate = new DateOnly(2027, 8, 31),
            HolidayAccountId = account.Id
        };

        // Act and assert
        await Assert.ThrowsAsync<ValidationException>(() => _requestService.CreateRequestAsync(dto, userId));
    }

    [Fact]
    public async Task Insert_AccountBelongsToAnotherUser_ThrowsException()
    {
        // Arrange
        var userId = "user123";
        var anotherUserId = "user456";

        var account = new HolidayAccount
        {
            Id = Guid.NewGuid(),
            UserId = anotherUserId,
            Type = HolidayType.Week1_5
        };

        await _accountRepoStub.InsertAsync(account);

        var dto = new CreateRequestDto
        {
            StartDate = new DateOnly(2027, 8, 31),
            EndDate = new DateOnly(2027, 9, 7),
            HolidayAccountId = account.Id
        };

        // Act and assert
        await Assert.ThrowsAsync<ValidationException>(() => _requestService.CreateRequestAsync(dto, userId));
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

    #endregion


    // *---* UPDATE TESTS *---* //

    #region Update Tests

    [Fact]
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
        await Assert.ThrowsAsync<ValidationException>(
            () => _requestService.UpdateRequestStatusAsync(
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
        await Assert.ThrowsAsync<ValidationException>(
            () => _requestService.CancelRequestAsync(
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
        await Assert.ThrowsAsync<ValidationException>(
            () => _requestService.CancelRequestAsync(
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
            HolidayAccountId = Guid.NewGuid(),
            StartDate = new DateOnly(2027, 8, 31),
            EndDate = new DateOnly(2027, 9, 7),
            CreatedAt = DateTime.UtcNow,
            Status = HolidayRequestStatus.Pending
        };
    }
}