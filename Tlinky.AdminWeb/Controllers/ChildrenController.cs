using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tlinky.AdminWeb.Data;
using Tlinky.AdminWeb.Models;

namespace Tlinky.AdminWeb.Controllers
{
    [Route("Children")]
    public class ChildrenController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ChildrenController(ApplicationDbContext context) => _context = context;

        // 🌼 Main page
        [HttpGet("")]
        public IActionResult Index() => View();

        // ✅ List all children
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var children = await _context.Children
                .Include(c => c.Class)
                .Include(c => c.Parent)
                .OrderBy(c => c.FullName)
                .Select(c => new
                {
                    c.ChildId,
                    c.FullName,
                    DOB = c.DOB.HasValue ? c.DOB.Value.ToString("yyyy-MM-dd") : "",
                    ClassName = c.Class != null ? c.Class.Name : "Unassigned",
                    ParentName = c.Parent != null ? c.Parent.FullName : "N/A",
                    c.Allergies,
                    c.Status,
                    c.PhotoUrl
                })
                .ToListAsync();

            return Json(children);
        }

        // ✅ Get children by class
        [HttpGet("/Children/GetByClass/{classId}")]
        public async Task<IActionResult> GetByClass(int classId)
        {
            var children = await _context.Children
                .Where(c => c.ClassId == classId && c.Status == "Active")
                .OrderBy(c => c.FullName)
                .Select(c => new
                {
                    c.ChildId,
                    c.FullName,
                    c.DOB,
                    c.Allergies,
                    c.Status,
                    c.PhotoUrl
                })
                .ToListAsync();

            return Json(children);
        }

        // ✅ Dropdown data (classes only)
        [HttpGet("Dropdowns")]
        public async Task<IActionResult> Dropdowns()
        {
            var classes = await _context.Classes
                .Where(c => c.Status == "Active")
                .Select(c => new { c.ClassId, c.Name })
                .ToListAsync();

            return Json(new { classes });
        }

        // ✅ Add child
        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] Child child)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest("Invalid child data.");

                // ✅ Use "Unspecified" kind to match PostgreSQL "timestamp without time zone"
                if (child.DOB.HasValue)
                {
                    child.DOB = DateTime.SpecifyKind(child.DOB.Value, DateTimeKind.Unspecified);
                }

                _context.Children.Add(child);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Child added successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error adding child: {ex}");
                return StatusCode(500, new { message = "Unexpected error while adding child.", error = ex.Message });
            }
        }

        // ✅ Edit child
        [HttpPut("Edit")]
        public async Task<IActionResult> Edit([FromBody] Child child)
        {
            try
            {
                var existing = await _context.Children.FindAsync(child.ChildId);
                if (existing == null) return NotFound(new { message = "Child not found." });

                existing.FullName = child.FullName;
                existing.Allergies = child.Allergies;
                existing.PhotoUrl = child.PhotoUrl;
                existing.Status = child.Status;
                existing.ClassId = child.ClassId;
                existing.ParentId = child.ParentId;

                // ✅ Safe DOB handling
                if (child.DOB.HasValue)
                {
                    existing.DOB = DateTime.SpecifyKind(child.DOB.Value, DateTimeKind.Unspecified);
                }
                else
                {
                    existing.DOB = null;
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Child updated successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error editing child: {ex}");
                return StatusCode(500, new { message = "Unexpected error while updating child.", error = ex.Message });
            }
        }

        // ✅ Delete child
        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var child = await _context.Children.FindAsync(id);
                if (child == null)
                    return NotFound(new { message = "Child not found." });

                _context.Children.Remove(child);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Child deleted successfully." });
            }
            catch (DbUpdateException dbEx)
            {
                // Handles FK constraint issues gracefully
                Console.WriteLine($"⚠️ DB delete constraint: {dbEx}");
                return BadRequest(new { message = "Unable to delete child — linked records exist (attendance, payments, etc.)." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error deleting child: {ex}");
                return StatusCode(500, new { message = "Unexpected error while deleting child.", error = ex.Message });
            }
        }
    }
}
