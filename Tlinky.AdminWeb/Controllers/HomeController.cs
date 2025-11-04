using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tlinky.AdminWeb.Data;

namespace Tlinky.AdminWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // 🏠 Simplified & Safe Dashboard (UTC-based)
        // =====================================================
        public async Task<IActionResult> Index()
        {
            var todayUtc = DateTime.UtcNow.Date;

            // ---------- SIMPLE COUNTS ----------
            var totalChildren = await _context.Children.CountAsync();
            var totalParents = await _context.Parents.CountAsync();
            var totalTeachers = await _context.Teachers.CountAsync();
            var totalIncidents = await _context.Incidents.CountAsync();

            // ---------- ATTENDANCE ----------
            var presentToday = await _context.Attendance
                .CountAsync(a => a.Date.Date == todayUtc && a.Status == "Present");

            var todaysAttendance = await _context.Attendance
                .Include(a => a.Child).ThenInclude(c => c.Class)
                .Where(a => a.Date.Date == todayUtc)
                .Select(a => new
                {
                    ChildName = a.Child.FullName,
                    ClassName = a.Child.Class.Name,
                    a.Status
                })
                .ToListAsync();

            // ---------- PAYMENTS ----------
            var outstandingFees = await _context.Payments
                .Where(p => p.Status == "Pending" || p.Status == "Unpaid")
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            var totalCollected = await _context.Payments
                .Where(p => p.Status == "Paid")
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            // ---------- MONTHLY TREND (safe client-side grouping) ----------
            var sixMonthsAgoUtc = todayUtc.AddMonths(-5);

            // get all recent payments first
            var recentPayments = await _context.Payments
                .Where(p => p.DateUploaded >= sixMonthsAgoUtc)
                .ToListAsync();

            // group & format in memory (EF-safe)
            var monthly = recentPayments
                .GroupBy(p => p.DateUploaded.ToString("MMM yyyy"))
                .Select(g => new
                {
                    Month = g.Key,
                    Total = g.Sum(x => x.Amount)
                })
                .OrderBy(g => g.Month)
                .ToList();

            // ---------- WEEKLY ATTENDANCE TREND ----------
            var weekStartUtc = todayUtc.AddDays(-6);

            var recentAttendance = await _context.Attendance
                .Where(a => a.Date >= weekStartUtc)
                .ToListAsync();

            var attendanceTrend = recentAttendance
                .GroupBy(a => a.Date.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Present = g.Count(x => x.Status == "Present")
                })
                .OrderBy(g => g.Date)
                .ToList();

            // ---------- PASS DATA TO VIEW ----------
            ViewBag.TotalChildren = totalChildren;
            ViewBag.TotalParents = totalParents;
            ViewBag.TotalTeachers = totalTeachers;
            ViewBag.TotalIncidents = totalIncidents;
            ViewBag.PresentToday = presentToday;
            ViewBag.OutstandingFees = outstandingFees;
            ViewBag.TotalCollected = totalCollected;
            ViewBag.TodaysAttendance = todaysAttendance;
            ViewBag.Monthly = monthly;
            ViewBag.AttendanceTrend = attendanceTrend;

            return View();
        }
    }
}
