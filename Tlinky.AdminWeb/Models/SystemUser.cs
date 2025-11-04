using System.ComponentModel.DataAnnotations;

namespace Tlinky.AdminWeb.Models
{
    public class SystemUser
    {
        [Key] public int UserId { get; set; }

        [Required] public string FullName { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "Admin";
        public string Status { get; set; } = "Active";
    }
}
