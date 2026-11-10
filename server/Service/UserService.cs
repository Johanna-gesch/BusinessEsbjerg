using System.ComponentModel.DataAnnotations;
using Infra.Entities;
using Infra.Repositories;
using Service.DTOs.UserDTOs;

namespace Service;

public class UserService(IUserRepo repo)
{
    public async Task<List<User>> GetAll() {
        return await repo.GetAll();
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
}
