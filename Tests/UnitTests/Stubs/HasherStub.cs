using Service.Security;

namespace Tests.UnitTests.Stubs;

public class HasherStub : IPasswordHasher
{
    public string HashAndSaltPassword(string password)
    {
        return $"hashed_{password}";
    }

    public string HashPassword(string password, string salt)
    {
        return $"hashed_{password}_{salt}";
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return hashedPassword == $"hashed_{password}";
    }
}