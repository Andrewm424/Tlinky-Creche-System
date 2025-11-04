using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;

namespace Tlinky.AdminWeb.Models
{
    public class Child
    {
        [Key] public int ChildId { get; set; }

        [Required] public string FullName { get; set; } = string.Empty;
        public DateTime? DOB { get; set; }

        [ForeignKey("Class")] public int? ClassId { get; set; }
        public Class? Class { get; set; }

        [ForeignKey("Parent")] public int? ParentId { get; set; }
        public Parent? Parent { get; set; }

        public string? Allergies { get; set; }
        public string? PhotoUrl { get; set; }
        public string Status { get; set; } = "Active";
    }
}
