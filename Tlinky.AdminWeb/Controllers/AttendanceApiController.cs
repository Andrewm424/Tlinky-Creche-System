using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using Tlinky.AdminWeb.Data;
using Tlinky.AdminWeb.Models;

namespace Tlinky.AdminWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeZoneInfo _saTimeZone;

        public AttendanceApiController(ApplicationDbContext context)
        {
            _context = context;

            // ✅ Handle both Windows & Linux timezone IDs
            _saTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "South Africa Standard Time"
                    : "Africa/Johannesburg"
            );
        }

        // ✅ GET: Filter by date and/or class (force UTC reading)
        // Example: /api/AttendanceApi?date=2025-10-24&classId=1
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] DateTime? date, [FromQuery] int? classId)
        {
            try
            {
                Console.WriteLine($"🔹 Fetching attendance for date={date}, classId={classId}");

                var targetDate = date?.Date ?? DateTime.UtcNow.Date;

                // ✅ 1. Get all children in this class
                var children = await _context.Children
                    .Include(c => c.Class)
                    .Where(c => classId == null || c.ClassId == classId)
                    .OrderBy(c => c.FullName)
                    .Select(c => new
                    {
                        c.ChildId,
                        ChildName = c.FullName,
                        ClassName = c.Class != null ? c.Class.Name : "N/A"
                    })
                    .ToListAsync();

                // ✅ 2. Get existing attendance for today
                var existing = await _context.Attendance
                    .Where(a => a.Date.Date == targetDate &&
                                (classId == null || a.Child.ClassId == classId))
                    .ToListAsync();

                // ✅ 3. Merge both lists — ensure each child appears once
                var merged = children.Select(child =>
                {
                    var record = existing.FirstOrDefault(a => a.ChildId == child.ChildId);
                    return new
                    {
                        ChildId = child.ChildId,
                        ChildName = child.ChildName,
                        ClassName = child.ClassName,
                        Date = targetDate.ToString("yyyy-MM-dd"),
                        Status = record?.Status ?? "Present",
                        Notes = record?.Notes ?? string.Empty,
                        AttendanceId = record?.AttendanceId
                    };
                }).ToList();

                Console.WriteLine($"✅ Loaded {merged.Count} attendance records (merged)");

                return Ok(new { success = true, count = merged.Count, data = merged });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR fetching attendance: {ex}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


        // ✅ GET: View child’s attendance history
        // Example: /api/AttendanceApi/child/3?days=30
        [HttpGet("child/{childId}")]
        public async Task<IActionResult> GetChildHistory(int childId, [FromQuery] int days = 30)
        {
            try
            {
                var since = DateTime.UtcNow.AddDays(-days);
                Console.WriteLine($"🔹 Fetching history for childId={childId}, since={since}");

                var history = await _context.Attendance
                    .Where(a => a.ChildId == childId && a.Date >= since)
                    .OrderByDescending(a => a.Date)
                    .Select(a => new
                    {
                        a.AttendanceId,
                        Date = DateTime.SpecifyKind(a.Date, DateTimeKind.Utc)
                            .ToString("yyyy-MM-dd"),
                        a.Status,
                        a.Notes,
                        Teacher = a.Teacher != null ? a.Teacher.FullName : "N/A"
                    })
                    .ToListAsync();

                Console.WriteLine($"✅ Loaded {history.Count} records for childId={childId}");
                return Ok(new { success = true, total = history.Count, data = history });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR fetching child history: {ex}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/AttendanceApi/summary?classId=3
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] int classId)
        {
            var today = DateTime.UtcNow.Date;
            var presentCount = await _context.Attendance
                .CountAsync(a => a.Child.ClassId == classId &&
                                 a.Date.Date == today &&
                                 a.Status == "Present");

            var totalChildren = await _context.Children.CountAsync(c => c.ClassId == classId);

            return Ok(new { totalChildren, presentCount });
        }


        // ✅ POST: Mark or update attendance
        // Flutter will POST a list of entries
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] List<Attendance>? records)
        {
            if (records == null || !records.Any())
                return BadRequest(new { success = false, message = "No attendance data received." });

            try
            {
                foreach (var record in records)
                {
                    // ✅ Normalize all incoming dates to UTC (no matter what Flutter sends)
                    if (record.Date.Kind == DateTimeKind.Unspecified)
                    {
                        // Assume it came in as local, convert to UTC
                        record.Date = DateTime.SpecifyKind(record.Date, DateTimeKind.Local).ToUniversalTime();
                    }
                    else if (record.Date.Kind == DateTimeKind.Local)
                    {
                        // Convert local to UTC
                        record.Date = record.Date.ToUniversalTime();
                    }
                    // else if already UTC, leave it as-is

                    // ✅ Set defaults
                    record.Status ??= "Present";
                    record.Notes ??= string.Empty;

                    if (record.ChildId <= 0)
                        continue;

                    // ✅ Check for existing record on same date
                    var existing = await _context.Attendance
                        .FirstOrDefaultAsync(a =>
                            a.ChildId == record.ChildId &&
                            a.Date.Date == record.Date.Date);

                    if (existing != null)
                    {
                        existing.Status = record.Status;
                        existing.Notes = record.Notes;
                        existing.TeacherId = record.TeacherId;
                    }
                    else
                    {
                        _context.Attendance.Add(record);
                    }
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ Attendance saved for {records.Count} record(s)");
                return Ok(new { success = true, message = "Attendance saved successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR saving attendance: {ex}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
