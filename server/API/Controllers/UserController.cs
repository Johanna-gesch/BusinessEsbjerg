using Infra.Entities;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace API.Controllers;

public class UserController(UserService service) : ControllerBase
{

    [HttpGet(nameof(GetUsers))]
    public async Task<List<User>> GetUsers()
    {
        return await service.GetAll();
    }

    [HttpGet(nameof(GetUserById))]
    public async Task<User> GetUserById(string id)
    {
        return await service.GetById(id);
    }

    [HttpPut(nameof(UpdateUser))]
    public async Task UpdateUser([FromBody]UpdateUserDto dto)
    {
        await service.Update(dto);
    }
    
}