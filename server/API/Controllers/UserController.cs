using Infra.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using Service.DTOs.UserDTOs;

namespace API.Controllers;

[ApiController]
public class UserController(UserService service, IAuthenticationService authenticationService) : ControllerBase
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

    [HttpDelete(nameof(DeleteUser))]
    public async Task DeleteUser(string userId)
    {
        await service.Delete(userId);
    }

    [Authorize]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDTO>> Login([FromBody]
        LoginRequestDTO request)
    {
        string? token = await authenticationService.Login(
            request.Username,
            request.Password);

        if (token == null)
        {
            return Unauthorized();
        }

        return Ok(new LoginResponseDTO
        {
            Token = token
        });
    }
    
    
    
}

public class LoginResponseDTO
{
    public string Token { get; set; } = string.Empty;
}

public class LoginRequestDTO
{
    public string Username { get; set; } = string.Empty;
    
    public string Password { get; set; } = string.Empty;

}