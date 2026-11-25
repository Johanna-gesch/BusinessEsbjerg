namespace Infra.Repositories.Interfaces;

/**
 * This interface is made for database transactions between classes.
 */
public interface IUnitOfWork
{
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}