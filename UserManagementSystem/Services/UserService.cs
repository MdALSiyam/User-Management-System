using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagementSystem.Models;
using UserManagementSystem.Repositories;

namespace UserManagementSystem.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository,
                           UserManager<IdentityUser> userManager,
                           SignInManager<IdentityUser> signInManager,
                           ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task<User> GetUserByIdAsync(Guid id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetUserByEmailAsync(email);
        }

        public async Task UpdateUserAsync(User user)
        {
            await _userRepository.UpdateUserAsync(user);
        }

        public async Task<IdentityResult> RegisterUserAsync(string name, string email, string password)
        {
            var existingCustomUser = await _userRepository.GetUserByEmailAsync(email);
            if (existingCustomUser != null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "An account with this email already exists in custom user profiles." });
            }

            var identityUser = new IdentityUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(identityUser, password);

            if (result.Succeeded)
            {
                try
                {
                    var customUser = new User
                    {
                        Id = Guid.Parse(identityUser.Id),
                        Name = name,
                        Email = email,
                        RegistrationTime = DateTime.UtcNow,
                        Status = "Active",
                        LastLoginTime = DateTime.UtcNow
                    };
                    await _userRepository.AddUserAsync(customUser);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Failed to save custom user profile for {Email}. Attempting to delete associated IdentityUser.", email);
                    await _userManager.DeleteAsync(identityUser);
                    if (IsUniqueConstraintViolation(ex))
                    {
                        return IdentityResult.Failed(new IdentityError { Description = "Email already exists." });
                    }
                    return IdentityResult.Failed(new IdentityError { Description = "An error occurred while saving user profile. Please try again." });
                }
            }
            return result;
        }

        public async Task<SignInResult> LoginUserAsync(string email, string password, bool isPersistent)
        {
            var identityUser = await _userManager.FindByEmailAsync(email);
            if (identityUser == null)
            {
                _logger.LogWarning("Login attempt failed for email {Email}: IdentityUser not found.", email);
                return SignInResult.Failed;
            }

            var customUser = await _userRepository.GetUserByIdAsync(Guid.Parse(identityUser.Id));
            if (customUser != null && customUser.Status == "Blocked")
            {
                _logger.LogWarning("Login attempt failed for email {Email}: Custom user status is 'Blocked'.", email);
                return SignInResult.NotAllowed;
            }

            var result = await _signInManager.PasswordSignInAsync(email, password, isPersistent, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                await UpdateLastLoginTimeAsync(email);
                _logger.LogInformation("User {Email} logged in successfully.", email);
            }
            else
            {
                _logger.LogWarning("Login attempt failed for email {Email}. Result: {SignInResult}", email, result.ToString());
            }

            return result;
        }

        public async Task UpdateLastLoginTimeAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user != null)
            {
                user.LastLoginTime = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);
                _logger.LogInformation("Last login time updated for user {Email}.", email);
            }
            else
            {
                _logger.LogWarning("Attempted to update last login time for non-existent user {Email}.", email);
            }
        }

        public async Task BlockUsersAsync(IEnumerable<Guid> userIds, Guid currentUserId)
        {
            if (userIds.Contains(currentUserId))
            {
                _logger.LogWarning("User {CurrentUserId} attempted to block their own account.", currentUserId);
                throw new ApplicationException("You cannot block your own account.");
            }

            foreach (var userId in userIds)
            {
                var customUser = await _userRepository.GetUserByIdAsync(userId);
                if (customUser != null)
                {
                    customUser.Status = "Blocked";
                    await _userRepository.UpdateUserAsync(customUser);
                    _logger.LogInformation("Custom user {UserId} status set to 'Blocked'.", userId);
                }

                var identityUser = await _userManager.FindByIdAsync(userId.ToString());
                if (identityUser != null)
                {
                    await _userManager.SetLockoutEnabledAsync(identityUser, true);
                    await _userManager.SetLockoutEndDateAsync(identityUser, DateTimeOffset.MaxValue);
                    _logger.LogInformation("IdentityUser {UserId} locked out indefinitely.", userId);
                }
                else
                {
                    _logger.LogWarning("IdentityUser not found for ID {UserId} during block operation.", userId);
                }
            }
        }

        public async Task UnblockUsersAsync(IEnumerable<Guid> userIds, Guid currentUserId)
        {
            foreach (var userId in userIds)
            {
                var customUser = await _userRepository.GetUserByIdAsync(userId);
                if (customUser != null)
                {
                    customUser.Status = "Active";
                    await _userRepository.UpdateUserAsync(customUser);
                    _logger.LogInformation("Custom user {UserId} status set to 'Active'.", userId);
                }

                var identityUser = await _userManager.FindByIdAsync(userId.ToString());
                if (identityUser != null)
                {
                    await _userManager.SetLockoutEndDateAsync(identityUser, null);
                    await _userManager.SetLockoutEnabledAsync(identityUser, false);
                    await _userManager.ResetAccessFailedCountAsync(identityUser);
                    _logger.LogInformation("IdentityUser {UserId} lockout removed and access failed count reset.", userId);
                }
                else
                {
                    _logger.LogWarning("IdentityUser not found for ID {UserId} during unblock operation.", userId);
                }
            }
        }

        public async Task DeleteUsersAsync(IEnumerable<Guid> userIds, Guid currentUserId)
        {
            if (userIds.Contains(currentUserId))
            {
                _logger.LogWarning("User {CurrentUserId} attempted to delete their own account.", currentUserId);
                throw new ApplicationException("You cannot delete your own account.");
            }

            foreach (var userId in userIds)
            {
                await _userRepository.DeleteUsersAsync(new List<Guid> { userId });
                _logger.LogInformation("Custom user {UserId} deleted from repository.", userId);

                var identityUser = await _userManager.FindByIdAsync(userId.ToString());
                if (identityUser != null)
                {
                    var result = await _userManager.DeleteAsync(identityUser);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("IdentityUser {UserId} deleted successfully.", userId);
                    }
                    else
                    {
                        _logger.LogError("Failed to delete IdentityUser {UserId}: {Errors}", userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    _logger.LogWarning("IdentityUser not found for ID {UserId} during delete operation. Custom user may have been deleted without corresponding Identity record.", userId);
                }
            }
        }

        public async Task<bool> IsUserBlockedAsync(Guid userId)
        {
            var customUser = await _userRepository.GetUserByIdAsync(userId);
            bool isCustomBlocked = customUser?.Status == "Blocked";

            var identityUser = await _userManager.FindByIdAsync(userId.ToString());
            bool isIdentityLockedOut = identityUser != null && await _userManager.IsLockedOutAsync(identityUser);

            return isCustomBlocked || isIdentityLockedOut;
        }
        private bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sqlEx &&
                   (sqlEx.Number == 2601 || sqlEx.Number == 2627);
        }
    }
}