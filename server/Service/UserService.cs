using Infra;
using Infra.Entities;
using Service.DTOs.UserDTOs;
using Infra.Repositories.Interfaces;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace Service;

public class UserService(IUserRepo repo)
{
    public async Task<List<User>> GetAll() {
        return await repo.GetAll();
    }

    public async Task<User> GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ValidationException("User id cannot be null or empty");
        
        return await repo.GetById(id)
            ?? throw new NotFoundException($"User with this id '{id}' not found");
    }
    
    public async Task Insert(CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            throw new ValidationException("Username and password are required");
        
        var newUser = new User
        {
            UserId = Guid.NewGuid().ToString(),
            Username = dto.Username,
            PasswordHash = dto.Password,
            IsAdmin = dto.IsAdmin,
        };
        await repo.Insert(newUser);
    }
    
    public async Task Update(UpdateUserDto dto)
    {
        var user = await repo.GetById(dto.UserId)
                   ?? throw new ValidationException("User not found");

        if (!string.IsNullOrWhiteSpace(dto.Username))
            user.Username = dto.Username;
        
        await repo.Update(user);
    }

    public async Task Delete(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new Infra.ValidationException("User id cannot be null or empty");

        var user = await repo.GetById(userId) ?? throw new NotFoundException($"User with id '{userId}' not found");

        await repo.Delete(user);
    }
}
