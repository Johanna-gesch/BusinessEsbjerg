using Infra.Entities;

namespace Service.Security;

public interface ITokenService
{
    string GenerateToken(User user);
}