using Infra.Entities;
using Infra.Repositories;
using Service.Security;

namespace Service;

public class AuthenticationService(ITokenService tokenService,
    IUserRepo userRepo,
    IPasswordHasher passwordHasher) : IAuthenticationService
{
    public async Task<string?> Login(
        string username,
        string password)
    {
        User? user = await userRepo.GetByUsername(username);

        if (user == null)
        {
            return null;
        }

        bool valid = passwordHasher.VerifyPassword(password,
            user.PasswordHash);

        if (!valid)
        {
            return null;
        }

        return tokenService.GenerateToken(user);
    }
}