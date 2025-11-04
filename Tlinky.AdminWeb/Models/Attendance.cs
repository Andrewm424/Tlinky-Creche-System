using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tlinky.AdminWeb.Models
{
    public class Attendance
    {
        [Key] public int AttendanceId { get; set; }

        [Required, ForeignKey("Child")] public int ChildId { get; set; }
        public Child? Child { get; set; }

        [ForeignKey("Teacher")] public int? TeacherId { get; set; }
        public Teacher? Teacher { get; set; }

        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTime Date { get; set; }

        [Required] public string Status { get; set; } = "Present";

        public string? Notes { get; set; }
    }
}
