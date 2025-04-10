using bd2.Application.Abstractions;
using bd2.Core.Exceptions;
using bd2.Infrastructure.DTO;

namespace bd2.Infrastructure.Repositories;

public class UserRepository(GenericRepository<UserDto> userRepository) : IUserRepository
{
    public void AddUser(UserDto user)
    {
        userRepository.Create(user);
    }

    public void UpdateUser(UserDto user)
    {
        userRepository.Update(user);
    }

    public UserDto? GetUser(string login, string hash)
    {
        return userRepository.ExecuteQuery<UserDto>(
            "SELECT * FROM Users WHERE Login = @login AND HashedPassword = @password",
            new Dictionary<string, object>
            {
                { "@login", login },
                { "@password", hash }
            }).FirstOrDefault();
    }
}