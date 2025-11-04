using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tlinky.AdminWeb.Data;
using Tlinky.AdminWeb.Models;

namespace Tlinky.AdminWeb.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnnouncementsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AnnouncementsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ GET: api/AnnouncementsApi
        [HttpGet]
        public async Task<IActionResult> GetAnnouncements([FromQuery] string? audience = null)
        {
            IQueryable<Announcement> query = _context.Announcements;

            if (!string.IsNullOrEmpty(audience))
            {
                query = query.Where(a =>
                    a.Audience == "Everyone" || a.Audience == audience);
            }

            var list = await query
                .OrderByDescending(a => a.DatePosted)
                .Select(a => new
                {
                    announcementId = a.AnnouncementId,
                    title = a.Title,
                    message = a.Message,
                    audience = a.Audience,
                    author = a.Author,
                    datePosted = a.DatePosted
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/AnnouncementsApi/count?audience=Teachers
        [HttpGet("count")]
        public async Task<IActionResult> GetCount([FromQuery] string? audience = "Teachers")
        {
            var since = DateTime.UtcNow.AddDays(-3); // "new" = last 3 days
            var count = await _context.Announcements
                .CountAsync(a => (a.Audience == "Everyone" || a.Audience == audience)
                                 && a.DatePosted >= since);
            return Ok(new { count });
        }


        // ✅ Optional: get one announcement by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAnnouncement(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
                return NotFound();

            return Ok(announcement);
        }
    }
}
