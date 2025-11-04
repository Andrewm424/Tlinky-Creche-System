using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tlinky.AdminWeb.Data;
using Tlinky.AdminWeb.Models;

namespace Tlinky.AdminWeb.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SettingsController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var setting = await _context.Settings.FirstOrDefaultAsync();
            if (setting == null)
            {
                setting = new SystemSetting();
                _context.Settings.Add(setting);
                await _context.SaveChangesAsync();
            }

            return View(setting);
        }

        [HttpPut("Settings/UpdateSchool")]
        public async Task<IActionResult> UpdateSchool([FromBody] SystemSetting updated)
        {
            var setting = await _context.Settings.FirstAsync();
            setting.SchoolName = updated.SchoolName;
            setting.Email = updated.Email;
            setting.Phone = updated.Phone;
            setting.Principal = updated.Principal;
            await _context.SaveChangesAsync();
            return Ok(new { message = "School info updated successfully" });
        }

        [HttpPut("Settings/UpdateFees")]
        public async Task<IActionResult> UpdateFees([FromBody] SystemSetting updated)
        {
            var setting = await _context.Settings.FirstAsync();
            setting.BaseMonthlyFee = updated.BaseMonthlyFee;
            setting.LateFeePolicy = updated.LateFeePolicy;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Fee settings updated successfully" });
        }

        [HttpPut("Settings/UpdatePrefs")]
        public async Task<IActionResult> UpdatePrefs([FromBody] SystemSetting updated)
        {
            var setting = await _context.Settings.FirstAsync();
            setting.NotificationsEnabled = updated.NotificationsEnabled;
            setting.TermDates = updated.TermDates;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Preferences updated successfully" });
        }
    }
}
