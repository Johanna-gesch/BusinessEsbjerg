using Infra.Repositories.Interfaces;

namespace Infra.Repositories.HolidayRepos;

public class UnitOfWork(MyDatabaseConnection dc) : IUnitOfWork
{
    public async Task BeginTransactionAsync()
    {
         await dc.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        await dc.CommitTransactionAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        await dc.RollbackTransactionAsync();
    }
}