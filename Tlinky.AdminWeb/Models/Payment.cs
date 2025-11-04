using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tlinky.AdminWeb.Models
{
    public class Payment
    {
        [Key] public int PaymentId { get; set; }

        [ForeignKey("Parent")] public int? ParentId { get; set; }
        public Parent? Parent { get; set; }

        [ForeignKey("Child")] public int? ChildId { get; set; }
        public Child? Child { get; set; }

        public string? Month { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";
        public string? ProofUrl { get; set; }

        [Column(TypeName = "timestamp with time zone")]
        public DateTime DateUploaded { get; set; } = DateTime.UtcNow;


    }
}
