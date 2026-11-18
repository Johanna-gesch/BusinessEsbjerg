using Infra.Entities.HolidayTracking.Requests;
using Infra.Repositories.Interfaces;
using LinqToDB;
using LinqToDB.Async;

namespace Infra.Repositories.HolidayRepos;


/**
 * Get and set requests. 
 */
public class RequestRepo(MyDatabaseConnection dc) : IRequestRepo
{
    public async Task Insert(HolidayRequest request)
    {
        await dc.InsertAsync(request);
    }

    public async Task<HolidayRequest?> GetById(Guid id)
    {
        return await dc.HolidayRequests.FirstOrDefaultAsync(hr => hr.Id == id);
    }

    public async Task Update(HolidayRequest request)
    {
        await dc.UpdateAsync(request);
    }

    public async Task<List<HolidayRequest>> GetAll()
    {
        return await dc.HolidayRequests
            .ToListAsync();
    }

    public async Task<List<HolidayRequest>> GetByUserId(string id)
    {
        return await dc.HolidayRequests
            .Where(hr => hr.UserId == id)
            .ToListAsync();
    }
}