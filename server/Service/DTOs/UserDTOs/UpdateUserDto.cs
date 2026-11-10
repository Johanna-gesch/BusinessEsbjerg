using LinqToDB.Mapping;

namespace Service.DTOs.UserDTOs;

public class UpdateUserDto
{
    [NotNull] public string UserId { get; set; } = "";
    public string Username { get; set; } = "";
}