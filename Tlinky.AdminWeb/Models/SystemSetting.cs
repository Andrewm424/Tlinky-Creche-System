using System.ComponentModel.DataAnnotations;

namespace Tlinky.AdminWeb.Models
{
    public class SystemSetting
    {
        [Key] public int SettingId { get; set; }

        // --- 🏫 School Info ---
        [Required] public string SchoolName { get; set; } = "Tlinky Crèche";
        [Required, EmailAddress] public string Email { get; set; } = "info@tlinky.org";
        [Required] public string Phone { get; set; } = "012-345-6789";
        [Required] public string Principal { get; set; } = "Mrs Dlamini Dzanibe";

        // --- 💰 Fees & Billing ---
        [Required, Range(0, 100000)]
        public decimal BaseMonthlyFee { get; set; } = 600;   // Default fallback
        [Required, Range(0, 100000)]
        public decimal ToddlerFee { get; set; } = 500;       // Age < 3
        [Required, Range(0, 100000)]
        public decimal PreschoolFee { get; set; } = 650;     // Age ≥ 3
        [Required, Range(0, 100000)]
        public decimal LateFeeAmount { get; set; } = 50;

        public bool LateFeePolicy { get; set; } = true;

        // --- ⚙️ Preferences ---
        public bool NotificationsEnabled { get; set; } = true;
        public string TermDates { get; set; } = "Jan – Dec 2025";
    }
}
