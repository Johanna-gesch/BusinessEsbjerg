using Infra.Entities.HolidayTracking.Accounts;
using Infra.Repositories.Interfaces;

namespace Tests.UnitTests.Stubs;

public class AccountRepoStub : IAccountRepo
{
    private readonly List<HolidayAccount> _accounts = new();
    
    public Task InsertAsync(HolidayAccount account)
    {
        _accounts.Add(account);
        return Task.CompletedTask;
    }

    public Task<HolidayAccount?> GetByIdAsync(Guid id)
    {
        var account = _accounts.FirstOrDefault(a => a.Id == id);
        return Task.FromResult(account);
    }

    public Task<ICollection<HolidayAccount>> GetUserAccountsAsync(string userId)
    {
        ICollection<HolidayAccount> accounts = _accounts
            .Where(a => a.UserId == userId)
            .ToList();

        return Task.FromResult(accounts);
    }
}