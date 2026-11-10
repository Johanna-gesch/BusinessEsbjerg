using Infra.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace API.Controllers;

[ApiController]
public class UserController(UserService service, IAuthenticationService authenticationService) : ControllerBase
{

    [HttpGet(nameof(GetUsers))]
    public async Task<List<User>> GetUsers()
    {
        return await service.GetAll();
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