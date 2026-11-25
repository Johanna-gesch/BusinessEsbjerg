using Infra.Entities.HolidayTracking.Transactions;
using Infra.Repositories.Interfaces;

namespace Tests.UnitTests.Stubs;

public class TransactionRepoStub : ITransactionRepo
{
    public List<HolidayTransaction> Transactions { get; } = new();

    public async Task<List<HolidayTransaction>> GetByAccount(Guid accountId)
    {
        return await Task.FromResult(
            Transactions
                .Where(t => t.HolidayAccountId == accountId)
                .ToList());
    }

    public Task Insert(HolidayTransaction transaction)
    {
        Transactions.Add(transaction);
        return Task.CompletedTask;
    }
}