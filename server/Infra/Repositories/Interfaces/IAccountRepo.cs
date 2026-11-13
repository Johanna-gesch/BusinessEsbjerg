using Infra.Entities.HolidayTracking.Accounts;

namespace Infra.Repositories.Interfaces;

public interface IAccountRepo
{
    Task InsertAsync(HolidayAccount account);
    Task<HolidayAccount?> GetByIdAsync(Guid id);
    Task<ICollection<HolidayAccount>> GetUserAccountsAsync(string userId);
}