using Infra.Entities.HolidayTracking.Requests;
using Infra.Repositories.Interfaces;

namespace Tests.UnitTests.Stubs;

public class RequestRepoStub : IRequestRepo
{
    private readonly List<HolidayRequest> _holidayRequests = new();
    
    public async Task Insert(HolidayRequest request)
    {
        _holidayRequests.Add(request);
        await Task.CompletedTask;
    }

    public async Task<HolidayRequest?> GetById(Guid id)
    {
        return await Task.FromResult(
            _holidayRequests.FirstOrDefault(hr => hr.Id == id));
    }

    public Task Update(HolidayRequest request)
    {
        var existingRequest = _holidayRequests.FirstOrDefault(
            hr => hr.Id == request.Id);

        if (existingRequest != null)
        {
            existingRequest.StartDate = request.StartDate;
            existingRequest.EndDate = request.EndDate;
            existingRequest.Status = request.Status;
        }

        return Task.CompletedTask;
    }

    public async Task<List<HolidayRequest>> GetAll()
    {
        return await Task.FromResult(_holidayRequests);
    }

    public async Task<List<HolidayRequest>> GetByUserId(string id)
    {
        return await Task.FromResult(
            _holidayRequests
                .Where(hr => hr.UserId == id)
                .ToList());
    } 
}