using ChatChallenge.Core.Entities;

namespace ChatChallenge.Core.Interfaces;

public interface IUserRepository
{
  Task<User?> GetUserByEmailAsync(string email);
  Task<User?> GetUserByUserNameAsync(string userName);
  Task<User> CreateUserAsync(User user, string password);
  Task<bool> ValidateUserCredentialsAsync(string email, string password);
}
