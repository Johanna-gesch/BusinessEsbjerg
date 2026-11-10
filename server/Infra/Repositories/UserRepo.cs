using Infra.Entities;

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

    public Task<List<User>> GetAll()
    {
        throw new NotImplementedException();
    }
}