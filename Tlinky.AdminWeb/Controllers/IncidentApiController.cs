using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tlinky.AdminWeb.Data;
using Tlinky.AdminWeb.Models;

namespace Tlinky.AdminWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IncidentApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public IncidentApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Incident record)
        {
            if (record == null)
                return BadRequest(new { success = false, message = "Invalid incident data." });

            try
            {
                // Validate
                var child = await _context.Children.FindAsync(record.ChildId);
                var teacher = await _context.Teachers.FindAsync(record.TeacherId);
                var classEntity = await _context.Classes.FindAsync(record.ClassId);

                if (child == null || teacher == null || classEntity == null)
                    return BadRequest(new { success = false, message = "Invalid references." });

                // Save
                record.Date = DateTime.UtcNow;
                _context.Incidents.Add(record);
                await _context.SaveChangesAsync();

                // Log notification
                _context.Notifications.Add(new Notification
                {
                    Type = "Incident",
                    Message = $"Incident logged by {teacher.FullName}: {record.Description} ({child.FullName})",
                    DateCreated = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)
                });
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Incident submitted successfully.",
                    incidentId = record.IncidentId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR saving incident: {ex}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
