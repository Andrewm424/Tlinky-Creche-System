using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Tlinky.AdminWeb.Models
{
    public class Parent
    {
        [Key] public int ParentId { get; set; }

        [Required] public string FullName { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] public string PasswordHash { get; set; } = string.Empty;

        public string? Phone { get; set; }
        public string Status { get; set; } = "Active";

        public ICollection<Child>? Children { get; set; }
        public ICollection<Payment>? Payments { get; set; }
    }
}
