using Infra.Entities;
using Infra.Repositories;
using Service.DTOs.UserDTOs;

namespace Service;

public class UserService(IUserRepo repo)
{

    public async Task Insert(CreateUserDto dto)
    {
        var newUser = new User
        {
            UserId = Guid.NewGuid().ToString(),
            Username = dto.Username,
        };
        await repo.Insert(newUser);
    }
}
