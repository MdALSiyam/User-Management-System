using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using UserManagementSystem.Models;

namespace UserManagementSystem.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(Guid id);
        Task<User> GetUserByEmailAsync(string email);
        Task UpdateUserAsync(User user);
        Task<IdentityResult> RegisterUserAsync(string name, string email, string password);
        Task<SignInResult> LoginUserAsync(string email, string password, bool isPersistent);
        Task UpdateLastLoginTimeAsync(string email);
        Task<bool> IsUserBlockedAsync(Guid userId);
        Task BlockUsersAsync(IEnumerable<Guid> userIds, Guid currentUserId);
        Task UnblockUsersAsync(IEnumerable<Guid> userIds, Guid currentUserId);
        Task DeleteUsersAsync(IEnumerable<Guid> userIds, Guid currentUserId);
    }
}