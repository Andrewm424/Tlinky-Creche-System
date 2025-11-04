using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tlinky.AdminWeb.Data;
using Tlinky.AdminWeb.Models;

namespace Tlinky.AdminWeb.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AttendanceController(ApplicationDbContext context) => _context = context;

        // 🏫 View: Attendance page (Admin portal)
        public async Task<IActionResult> Index()
        {
            // Load basic data if you want (optional)
            var classes = await _context.Classes
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.Classes = classes;
            return View();
        }

        // 🧾 Optional: For server-rendered lists (if ever needed)
        [HttpGet("Attendance/GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.Attendance
                .Include(a => a.Child)
                .ThenInclude(c => c.Class)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            return Json(list);
        }

        // 🧠 Optional: fallback for direct web updates (not used by JS)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAttendance(List<Attendance> records)
        {
            if (records == null || !records.Any())
                return RedirectToAction("Index");

            foreach (var record in records)
            {
                record.Date = DateTime.SpecifyKind(record.Date.ToUniversalTime(), DateTimeKind.Utc);
                record.Status ??= "Present";
                record.Notes ??= string.Empty;

                var existing = await _context.Attendance
                    .FirstOrDefaultAsync(a =>
                        a.ChildId == record.ChildId &&
                        a.Date.Date == record.Date.Date);

                if (existing != null)
                {
                    existing.Status = record.Status;
                    existing.Notes = record.Notes;
                }
                else
                {
                    _context.Attendance.Add(record);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = "Attendance saved successfully!";
            return RedirectToAction("Index");
        }
    }
}
