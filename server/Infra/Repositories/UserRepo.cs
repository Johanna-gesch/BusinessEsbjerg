using Infra.Entities;
using LinqToDB;

namespace Infra.Repositories;

public class UserRepo(MyDatabaseConnection dc) : IUserRepo
{
    public async Task Insert(User user)
    {
        await dc.InsertAsync(user);
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

    public Task<List<User>> GetAll()
    {
        throw new NotImplementedException();
    }
}