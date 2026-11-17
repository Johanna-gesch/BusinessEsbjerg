using Infra.Entities.Users;

namespace Service.DTOs.UserDTOs;

public class CreateUserDto
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public UserRole Role { get; set; } 
}