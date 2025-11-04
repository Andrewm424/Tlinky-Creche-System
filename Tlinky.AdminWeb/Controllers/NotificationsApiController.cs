using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tlinky.AdminWeb.Data;
using Tlinky.AdminWeb.Models;

namespace Tlinky.AdminWeb.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public NotificationsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Get all notifications (latest 10)
        [HttpGet]
        public async Task<IActionResult> GetLatest()
        {
            var data = await _context.Notifications
                .OrderByDescending(n => n.DateCreated)
                .Take(10)
                .ToListAsync();

            return Ok(data);
        }

        // ✅ Add notification (when payment or incident happens)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Notification model)
        {
            if (string.IsNullOrWhiteSpace(model.Message))
                return BadRequest("Message required");

            model.DateCreated = DateTime.UtcNow;
            _context.Notifications.Add(model);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // ✅ Clear all notifications
        [HttpDelete]
        public async Task<IActionResult> ClearAll()
        {
            _context.Notifications.RemoveRange(_context.Notifications);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}
