using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Tlinky.AdminWeb.Data;
using Tlinky.AdminWeb.Models;

namespace Tlinky.AdminWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParentApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ParentApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Parent Login (Flutter-compatible)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { success = false, message = "Invalid credentials." });

            var hash = HashPassword(model.Password);

            var parent = await _context.Parents
                .Include(p => p.Children)
                .ThenInclude(c => c.Class)
                .FirstOrDefaultAsync(p =>
                    p.Email.ToLower() == model.Email.ToLower() &&
                    p.PasswordHash == hash);

            if (parent == null)
                return Unauthorized(new { success = false, message = "Invalid email or password." });

            // ✅ Pick first active child for display
            var child = parent.Children?.FirstOrDefault(c => c.Status == "Active");

            return Ok(new
            {
                success = true,
                parentId = parent.ParentId,
                fullName = parent.FullName,
                email = parent.Email,
                phone = parent.Phone,
                status = parent.Status,

                // ✅ Flattened child details (for easy Flutter mapping)
                childName = child?.FullName ?? "Unknown Child",
                className = child?.Class?.Name ?? "Unassigned Class",
                allergies = child?.Allergies ?? "None",
                childAge = child?.DOB != null
                    ? (int)((DateTime.Now - child.DOB.Value).TotalDays / 365.25)
                    : (int?)null,
                childPhoto = child?.PhotoUrl,

                // ✅ Also return children list (for multi-child support)
                children = parent.Children.Select(c => new
                {
                    c.ChildId,
                    c.FullName,
                    ClassName = c.Class != null ? c.Class.Name : "Unassigned",
                    c.Status
                }).ToList()
            });
        }

        // ✅ Get child details
        [HttpGet("children/{parentId}")]
        public async Task<IActionResult> GetChildren(int parentId)
        {
            var parent = await _context.Parents
                .Include(p => p.Children)
                .ThenInclude(c => c.Class)
                .FirstOrDefaultAsync(p => p.ParentId == parentId);

            if (parent == null)
                return NotFound(new { message = "Parent not found." });

            var children = parent.Children.Select(c => new
            {
                c.ChildId,
                c.FullName,
                ClassName = c.Class != null ? c.Class.Name : "N/A",
                c.Status
            });

            return Ok(children);
        }

        // ✅ Parent Dashboard Overview (for Flutter Dashboard)
        // Example: /api/ParentApi/Overview/3
        [HttpGet("Overview/{parentId}")]
        public async Task<IActionResult> GetOverview(int parentId)
        {
            try
            {
                var parent = await _context.Parents
                    .Include(p => p.Children)
                    .FirstOrDefaultAsync(p => p.ParentId == parentId);

                if (parent == null)
                    return NotFound(new { success = false, message = "Parent not found." });

                var childIds = parent.Children.Select(c => c.ChildId).ToList();

                // ✅ Payments Summary
                var payments = await _context.Payments
                    .Where(p => p.ParentId == parentId)
                    .ToListAsync();

                decimal totalFees = payments.Any() ? payments.Sum(p => p.Amount) : 0m;
                decimal totalPaid = payments.Where(p => p.Status == "Paid").Sum(p => p.Amount);
                decimal balance = totalFees - totalPaid;

                // ✅ Attendance Summary (for all children)
                var attendanceRecords = await _context.Attendance
                    .Where(a => childIds.Contains(a.ChildId))
                    .ToListAsync();

                var presentCount = attendanceRecords.Count(a => a.Status == "Present");
                var totalCount = attendanceRecords.Count();
                double attendanceRate = totalCount > 0
                    ? Math.Round((double)presentCount / totalCount * 100, 1)
                    : 0.0;

                return Ok(new
                {
                    success = true,
                    totalChildren = parent.Children.Count,
                    totalFees,
                    totalPaid,
                    balance,
                    attendanceRate
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR building parent overview: {ex}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ✅ Password Hash Helper
        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        // ✅ DTO for login
        public class LoginDto
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
    }
}
