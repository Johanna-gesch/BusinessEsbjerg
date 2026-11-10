namespace Service.Security;

public interface IPasswordHasher
{
    public string HashAndSaltPassword(string password);

    public string HashPassword(string password, string salt);

    public bool VerifyPassword(string password, string hashedPassword);

}