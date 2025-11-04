using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tlinky.AdminWeb.Data;
using Tlinky.AdminWeb.Models;

namespace Tlinky.AdminWeb.Controllers
{
    public class ClassesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClassesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();

        // ✅ Get all classes
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var classes = await _context.Classes
                .Include(c => c.Teacher)
                .OrderBy(c => c.Name)
                .Select(c => new
                {
                    c.ClassId,
                    c.Name,
                    TeacherName = c.Teacher != null ? c.Teacher.FullName : "Unassigned",
                    StudentCount = _context.Children.Count(ch => ch.ClassId == c.ClassId),
                    c.Status
                })
                .ToListAsync();

            return Json(classes);
        }

        // ✅ Dropdown list for teachers
        [HttpGet]
        public async Task<IActionResult> Dropdowns()
        {
            var teachers = await _context.Teachers
                .Where(t => t.Status == "Active")
                .Select(t => new { t.TeacherId, t.FullName })
                .ToListAsync();

            return Json(new { teachers });
        }

        // ✅ Add class
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Class cls)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid data.");

            _context.Classes.Add(cls);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Class added successfully." });
        }

        // ✅ Edit class
        [HttpPut]
        public async Task<IActionResult> Edit([FromBody] Class cls)
        {
            var existing = await _context.Classes.FindAsync(cls.ClassId);
            if (existing == null) return NotFound();

            existing.Name = cls.Name;
            existing.TeacherId = cls.TeacherId;
            existing.Status = cls.Status;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Class updated successfully." });
        }

        // ✅ Delete class
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var cls = await _context.Classes.FindAsync(id);
            if (cls == null)
                return NotFound(new { message = "Class not found." });

            _context.Classes.Remove(cls);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Class deleted successfully." });
        }
    }
}
