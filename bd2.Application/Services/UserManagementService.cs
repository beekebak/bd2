using System.Security.Cryptography;
using System.Text;
using bd2.Application.Abstractions;
using bd2.Infrastructure.DTO;

namespace bd2.Application.Services;

public class UserManagementService(IUserRepository repository)
{
    public void RegisterUser(string login, string plainPassword, string role)
    {
        repository.AddUser(new UserDto
        {
            Id = 0,
            HashedPassword = HashPassword(plainPassword),
            Login = login,
            Role = role
        });
    }

    public bool UpdateUser(int id, string oldLogin, string newLogin, string oldPlainPassword, string newPlainPassword, string role)
    {
        if (repository.GetUser(oldLogin, HashPassword(oldPlainPassword)) == null) return false;
        repository.UpdateUser(new UserDto
        {
            Id = id,
            Login = newLogin,
            Role = role,
            HashedPassword = HashPassword(newPlainPassword)
        });
        return true;
    }

    public UserDto? GetUserPlainPassword(string login, string plainPassword)
    {
        return repository.GetUser(login, HashPassword(plainPassword));
    }
    
    public UserDto? GetUserHashedPassword(string login, string hashedPassword)
    {
        return repository.GetUser(login, hashedPassword);
    }
        
    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hash);
    }
}