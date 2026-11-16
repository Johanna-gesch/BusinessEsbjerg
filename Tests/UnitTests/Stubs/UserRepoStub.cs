using Infra.Entities;
using Infra.Repositories.Interfaces;

namespace Tests.UnitTests.Stubs;

public class UserRepoStub : IUserRepo
{
    private readonly List<User> _users = new();
    public async Task Insert(User user)
    {
        _users.Add(user);
        await Task.CompletedTask;
    }

    public async Task<User?> GetById(string id)
    {
        return await Task.FromResult(
            _users.FirstOrDefault(u => u.UserId == id)
        );
    }

    public Task Update(User user)
    {
        var existingUser = _users.FirstOrDefault(
            u => u.UserId == user.UserId
        );

        if (existingUser != null)
        {
            existingUser.Username = user.Username;
            existingUser.PasswordHash = user.PasswordHash;
            existingUser.IsAdmin = user.IsAdmin;
        }

        return Task.CompletedTask;
    }

    public Task Delete(User user)
    {
        _users.Remove(user);
        return Task.CompletedTask;
    }

    public async Task<List<User>> GetAll()
    {
        return await Task.FromResult(_users);
        
    }

    public async Task<User?> GetByUsername(string username)
    {
        return await Task.FromResult(
            _users.FirstOrDefault(u => u.Username == username)
        );
    }
}