using Infra.Entities;

namespace Infra.Repositories;

public interface IUserRepo
{
    Task Insert(User user);
    Task<User> GetById(string id);
    Task Update(User user);
    Task Delete(User user);
    Task<List<User>> GetAll();
    Task<User?> GetByUsername(string username);
}