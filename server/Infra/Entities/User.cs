using LinqToDB.Mapping;

namespace Infra.Entities;

public class User
{
    [PrimaryKey] public string UserId { get; set; } = "";
    [Column] public string Username { get; set; } = "";
    [Column] public string PasswordHash { get; set; } = "";
    [Column] public bool IsAdmin { get; set; } = false;
    [Column] public bool IsActive { get; set; } = true;
}