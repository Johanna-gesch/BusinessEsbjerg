using Infra.Entities.HolidayTracking.Requests;

namespace Infra.Repositories.Interfaces;

public interface IRequestRepo
{
    Task Insert(HolidayRequest request);
    Task<HolidayRequest?> GetById(Guid id);
    Task Update(HolidayRequest request);
    Task<List<HolidayRequest>> GetAll();
    Task<List<HolidayRequest>> GetByUserId(string id);
}