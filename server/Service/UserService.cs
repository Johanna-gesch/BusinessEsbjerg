using Infra;
using Infra.Entities;
using Infra.Repositories;
using Infra.Repositories.Interfaces;
using LinqToDB.Internal.SqlQuery;
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

    public async Task Update(UpdateUserDto dto)
    {
        var user = await repo.GetById(dto.UserId)
            ?? throw new ValidationException("User not found");

        if (!string.IsNullOrWhiteSpace(dto.Username))
            user.Username = dto.Username;
        
        await repo.Update(user);
    }
    
    

}
