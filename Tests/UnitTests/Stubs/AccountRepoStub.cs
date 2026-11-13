using Infra.Entities.HolidayTracking.Accounts;
using Infra.Repositories.Interfaces;

namespace Tests.UnitTests.Stubs;

public class AccountRepoStub : IAccountRepo
{
    public List<HolidayAccount> Accounts { get; } = new();
    
    public Task InsertAsync(HolidayAccount account)
    {
        Accounts.Add(account);
        return Task.CompletedTask;
    }

    public Task<HolidayAccount?> GetByIdAsync(Guid id)
    {
        var account = Accounts.FirstOrDefault(a => a.Id == id);
        return Task.FromResult(account);
    }

    public Task<ICollection<HolidayAccount>> GetUserAccountsAsync(string userId)
    {
        ICollection<HolidayAccount> accounts = Accounts
            .Where(a => a.UserId == userId)
            .ToList();

        return Task.FromResult(accounts);
    }
}