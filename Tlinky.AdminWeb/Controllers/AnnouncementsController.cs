using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tlinky.AdminWeb.Data;
using Tlinky.AdminWeb.Models;

namespace Tlinky.AdminWeb.Controllers
{
    public class AnnouncementsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnnouncementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🌼 Main Page
        public IActionResult Index() => View();

        // ✅ Get all announcements
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.Announcements
                .OrderByDescending(a => a.DatePosted)
                .Select(a => new
                {
                    a.AnnouncementId,
                    a.Title,
                    a.Message,
                    a.Audience,
                    a.Author,
                    a.DatePosted
                })
                .ToListAsync();

            return Json(list);
        }

        // ✅ Add new announcement
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Announcement model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid announcement data");

            model.DatePosted = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

            _context.Announcements.Add(model);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Announcement added successfully" });
        }

        // ✅ Delete announcement
        [HttpDelete("Announcements/Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
                return NotFound(new { message = "Announcement not found" });

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Announcement deleted successfully" });
        }
    }
}
