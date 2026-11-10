namespace Service;

public interface IAuthenticationService
{
    Task<string?> Login(string username, string password);
}