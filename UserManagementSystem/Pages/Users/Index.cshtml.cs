using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using UserManagementSystem.Services;
using UserManagementSystem.Models;

namespace UserManagementSystem.Pages.Users
{
    public class IndexModel : PageModel
    {
        private readonly IUserService _userService;

        public IndexModel(IUserService userService)
        {
            _userService = userService;
        }

        public List<UserDataViewModel> Users { get; set; }
        public string StatusMessage { get; set; }
        public bool IsSuccess { get; set; }
        public class UserDataViewModel
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public DateTime? LastLogin { get; set; }
            public string Status { get; set; }
        }

        public async Task OnGetAsync()
        {
            await LoadUsersAsync();
        }
        public async Task<IActionResult> OnPostBlockAsync(List<Guid> selectedUserIds)
        {
            if (selectedUserIds == null || selectedUserIds.Count == 0)
            {
                StatusMessage = "Please select at least one user to block.";
                IsSuccess = false;
                await LoadUsersAsync();
                return Page();
            }

            try
            {
                Guid currentUserId = Guid.Empty;
                await _userService.BlockUsersAsync(selectedUserIds, currentUserId);
                StatusMessage = $"{selectedUserIds.Count} user(s) blocked successfully.";
                IsSuccess = true;
            }
            catch (ApplicationException ex)
            {
                StatusMessage = ex.Message;
                IsSuccess = false;
            }
            catch (Exception ex)
            {
                StatusMessage = $"An error occurred while blocking users: {ex.Message}";
                IsSuccess = false;
            }

            await LoadUsersAsync();
            return Page();
        }
        public async Task<IActionResult> OnPostUnblockAsync(List<Guid> selectedUserIds)
        {
            if (selectedUserIds == null || selectedUserIds.Count == 0)
            {
                StatusMessage = "Please select at least one user to unblock.";
                IsSuccess = false;
                await LoadUsersAsync();
                return Page();
            }

            try
            {
                Guid currentUserId = Guid.Empty;
                await _userService.UnblockUsersAsync(selectedUserIds, currentUserId);
                StatusMessage = $"{selectedUserIds.Count} user(s) unblocked successfully.";
                IsSuccess = true;
            }
            catch (ApplicationException ex)
            {
                StatusMessage = ex.Message;
                IsSuccess = false;
            }
            catch (Exception ex)
            {
                StatusMessage = $"An error occurred while unblocking users: {ex.Message}";
                IsSuccess = false;
            }

            await LoadUsersAsync();
            return Page();
        }
        public async Task<IActionResult> OnPostDeleteAsync(List<Guid> selectedUserIds)
        {
            if (selectedUserIds == null || selectedUserIds.Count == 0)
            {
                StatusMessage = "Please select at least one user to delete.";
                IsSuccess = false;
                await LoadUsersAsync();
                return Page();
            }

            try
            {
                Guid currentUserId = Guid.Empty; 
                await _userService.DeleteUsersAsync(selectedUserIds, currentUserId);
                StatusMessage = $"{selectedUserIds.Count} user(s) deleted successfully.";
                IsSuccess = true;
            }
            catch (ApplicationException ex)
            {
                StatusMessage = ex.Message;
                IsSuccess = false;
            }
            catch (Exception ex)
            {
                StatusMessage = $"An error occurred while deleting users: {ex.Message}";
                IsSuccess = false;
            }

            await LoadUsersAsync();
            return Page();
        }

        private async Task LoadUsersAsync()
        {
            var allUsers = await _userService.GetAllUsersAsync();
            Users = new List<UserDataViewModel>();

            foreach (var user in allUsers)
            {
                Users.Add(new UserDataViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    LastLogin = user.LastLoginTime.HasValue
                        ? user.LastLoginTime.Value.ToLocalTime()
                        : (DateTime?)null,
                    Status = user.Status
                });
            }
        }
    }
}