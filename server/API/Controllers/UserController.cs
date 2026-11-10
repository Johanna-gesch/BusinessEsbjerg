using Infra.Entities;
using Microsoft.AspNetCore.Mvc;
using Service;
using Service.DTOs.UserDTOs;

namespace API.Controllers;

public class UserController(UserService service) : ControllerBase
{
    [HttpPost(nameof(CreateUser))]
    public async Task CreateUser([FromBody] CreateUserDto dto)
    {
        await service.Insert(dto);
    }

    [HttpGet(nameof(GetUsers))]
    public async Task<List<User>> GetUsers()
    {
        return await service.GetAll();
    }
    
}