using Infra.Repositories.Interfaces;

namespace Tests.UnitTests.Stubs;

public class UnitOfWorkStub : IUnitOfWork
{
    public bool TransactionStarted { get; private set; }
    public bool TransactionCommitted { get; private set; }
    public bool TransactionRolledBack { get; private set; }

    public Task BeginTransactionAsync()
    {
        TransactionStarted = true;
        return Task.CompletedTask;
    }

    public Task CommitTransactionAsync()
    {
        TransactionCommitted = true;
        return Task.CompletedTask;
    }

    public Task RollbackTransactionAsync()
    {
        TransactionRolledBack = true;
        return Task.CompletedTask;
    }
}
