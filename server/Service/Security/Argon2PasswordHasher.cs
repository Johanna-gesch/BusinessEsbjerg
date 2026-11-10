using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace Service.Security;

public class Argon2PasswordHasher : IPasswordHasher
{
    public string HashAndSaltPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);

        return HashPassword(password, Convert.ToBase64String(salt));
    }

    public string HashPassword(string password, string salt)
    {
        byte[] hash = ComputeHash(
            password,
            Convert.FromBase64String(salt));

        return $"{Convert.ToBase64String(hash)}.{salt}";
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new InvalidDataException("The password needs to be set");
        }

        if (string.IsNullOrEmpty(hashedPassword))
        {
            throw new InvalidDataException("The hashedPassword isn't set.");
        }

        string[] parts = hashedPassword.Split('.');
        if (parts.Length != 2)
        {
            throw new InvalidDataException("Can't differentiate the hashed password.");
        }
        
        //Wrapping these will make the method return false instead of crashing. 
        byte[] storedHash;
        byte[] salt;
        try
        {
            storedHash = Convert.FromBase64String(parts[0]);
            salt = Convert.FromBase64String(parts[1]);
        }
        catch (FormatException)
        {
            return false;
        }

        if (storedHash.Length != 32 || salt.Length != 16)
        {
            return false;
        }

        byte[] computeHash = ComputeHash(password, salt);

        return CryptographicOperations.FixedTimeEquals(storedHash, computeHash);

    }

    private byte[] ComputeHash(string password, byte[] salt)
    {
        Argon2id argon2 = new Argon2id(
            Encoding.UTF8.GetBytes(password));

        argon2.Salt = salt;
        argon2.MemorySize = 64 * 1024;
        argon2.Iterations = 3;
        argon2.DegreeOfParallelism = 4;

        return argon2.GetBytes(32);
    }
}