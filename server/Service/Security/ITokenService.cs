using Infra.Entities;
using Infra.Entities.Users;

namespace Service.Security;

public interface ITokenService
{
    string GenerateToken(User user);
}