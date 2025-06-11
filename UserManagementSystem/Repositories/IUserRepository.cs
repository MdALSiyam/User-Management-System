using UserManagementSystem.Models;

namespace UserManagementSystem.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(Guid id);
        Task<User> GetUserByEmailAsync(string email);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(Guid id);
        Task BlockUsersAsync(IEnumerable<Guid> userIds);
        Task UnblockUsersAsync(IEnumerable<Guid> userIds);
        Task DeleteUsersAsync(IEnumerable<Guid> userIds);
    }
}
