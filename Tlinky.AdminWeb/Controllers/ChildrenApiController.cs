using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tlinky.AdminWeb.Data;

namespace Tlinky.AdminWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChildrenApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ChildrenApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ GET: /api/ChildrenApi?classId=1
        [HttpGet]
        public async Task<IActionResult> GetChildren([FromQuery] int? classId)
        {
            try
            {
                var query = _context.Children
                    .Include(c => c.Class)
                    .AsQueryable();

                if (classId.HasValue)
                    query = query.Where(c => c.ClassId == classId.Value);

                var data = await query
                    .OrderBy(c => c.FullName)
                    .Select(c => new
                    {
                        c.ChildId,
                        c.FullName,
                        ClassName = c.Class != null ? c.Class.Name : "Unassigned",
                        c.Status,
                        c.Allergies,
                        c.PhotoUrl
                    })
                    .ToListAsync();

                return Ok(new { success = true, count = data.Count, data });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR fetching children: {ex}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
