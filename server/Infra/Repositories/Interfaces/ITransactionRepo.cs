using Infra.Entities.HolidayTracking.Transactions;

namespace Infra.Repositories.Interfaces;

public interface ITransactionRepo
{
    Task<List<HolidayTransaction>> GetByAccount(Guid accountId);
}
