using Infra.Entities;
using Service.Security;

namespace Service;

public class AuthenticationService(ITokenService tokenService) : IAuthenticationService
{
    public async Task<string?> Login(
        string username,
        string password)
}