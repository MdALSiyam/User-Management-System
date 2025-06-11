using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace UserManagementSystem.Models
{
    public class User : IdentityUser<Guid>
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public DateTime RegistrationTime { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";
        public string? ActivityData { get; set; }
        public User()
        {

        }
    }
}