using Infra.Entities;
using Service.Security;

namespace Tests.UnitTests.Stubs;

public class TokenServiceStub : ITokenService
{
    public int GenerateTokenCallCount { get; private set; }

    public User? GeneratedForUser { get; private set; }

    public string GenerateToken(User user)
    {
        GenerateTokenCallCount++;
        GeneratedForUser = user;

        return "fake-jwt-token";
    }
}