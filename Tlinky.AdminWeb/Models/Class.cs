using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tlinky.AdminWeb.Models
{
    public class Class
    {
        [Key] public int ClassId { get; set; }

        [Required] public string Name { get; set; } = string.Empty;

        [ForeignKey("Teacher")] public int? TeacherId { get; set; }
        public Teacher? Teacher { get; set; }

        public string Status { get; set; } = "Active";

        public ICollection<Child>? Children { get; set; }
    }
}
