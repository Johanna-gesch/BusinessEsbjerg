using Infra.Entities;
using LinqToDB.Async;

namespace Infra.Repositories;

public class UserRepo(MyDatabaseConnection dc) : IUserRepo
{
    public Task Insert(User user)
    {
        throw new NotImplementedException();
    }

    public Task<User> GetById(string id)
    {
        throw new NotImplementedException();
    }

    public Task Update(User user)
    {
        throw new NotImplementedException();
    }

    public Task Delete(User user)
    {
        throw new NotImplementedException();
    }

    public async Task<List<User>> GetAll()
    {
        return await dc.Users.ToListAsync();
    }
}