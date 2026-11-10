using Infra.Entities;
using Infra.Repositories.Interfaces;
using LinqToDB;
using LinqToDB.Async;
using LinqToDB;

namespace Infra.Repositories;

public class UserRepo(MyDatabaseConnection dc) : IUserRepo
{
    public async Task Insert(User user)
    {
        await dc.InsertAsync(user);
    }

    public async Task<User?> GetById(string id)
    {
        return await dc.Users.FirstOrDefaultAsync(u => u.UserId == id);
    }

    public async Task Update(User user)
    {
        await dc.UpdateAsync(user);
    }

    public async Task Delete(User user)
    {
        user.IsActive = false;

        await dc.UpdateAsync(user);
    }

    public async Task<List<User>> GetAll()
    {
        return await dc.Users
            .Where(u => u.IsActive)
            .ToListAsync();
    }

    public Task<User?> GetByUsername(string username)
    {
        throw new NotImplementedException();
    }
}