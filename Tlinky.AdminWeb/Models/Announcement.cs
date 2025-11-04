using System;
using System.ComponentModel.DataAnnotations;

namespace Tlinky.AdminWeb.Models
{
    public class Announcement
    {
        [Key] public int AnnouncementId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public string Audience { get; set; } = "Everyone"; // Everyone | Teachers | Parents

        [Required]
        public DateTime DatePosted { get; set; } = DateTime.Now;



        public string? Author { get; set; }
    }
}
