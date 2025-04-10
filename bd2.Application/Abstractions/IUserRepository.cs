using bd2.Infrastructure.DTO;

namespace bd2.Application.Abstractions;

public interface IUserRepository
{
    public void AddUser(UserDto user);
    public void UpdateUser(UserDto user);
    public UserDto? GetUser(string login, string hash);
}