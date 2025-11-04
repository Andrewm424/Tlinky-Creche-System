using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tlinky.AdminWeb.Models
{
    public class Incident
    {
        [Key] public int IncidentId { get; set; }
        [Required] public DateTime Date { get; set; } = DateTime.UtcNow;
        [Required, StringLength(100)] public string Type { get; set; } = string.Empty;
        [Required, StringLength(500)] public string Description { get; set; } = string.Empty;

        [ForeignKey("Child")] public int? ChildId { get; set; }   // ✅ NEW
        public Child? Child { get; set; }

        [ForeignKey("Class")] public int? ClassId { get; set; }
        public Class? Class { get; set; }

        [ForeignKey("Teacher")] public int? TeacherId { get; set; }
        public Teacher? Teacher { get; set; }
    }

}

