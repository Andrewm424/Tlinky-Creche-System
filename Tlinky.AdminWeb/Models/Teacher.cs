using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tlinky.AdminWeb.Models
{
    public class Teacher
    {
        [Key] public int TeacherId { get; set; }

        [Required] public string FullName { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] public string PasswordHash { get; set; } = string.Empty;

        public string? PhotoUrl { get; set; }
        public string Status { get; set; } = "Active";

        public ICollection<Class>? Classes { get; set; }
        public ICollection<Attendance>? AttendanceRecords { get; set; }
    }
}
