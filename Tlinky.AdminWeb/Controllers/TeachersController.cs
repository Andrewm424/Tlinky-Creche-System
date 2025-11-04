using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Tlinky.AdminWeb.Data;
using Tlinky.AdminWeb.Models;

namespace Tlinky.AdminWeb.Controllers
{
    [Route("[controller]")]
    public class TeachersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeachersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // 🖥️ PAGE: Render the Teachers View (fix for 405 error)
        // =====================================================
        [HttpGet("")]
        public IActionResult Index()
        {
            return View(); // returns Views/Teachers/Index.cshtml
        }

        // =====================================================
        // 🔒 UTILITY: Hash Passwords using SHA256
        // =====================================================
        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        // =====================================================
        // 📋 GET ALL TEACHERS
        // =====================================================
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var teachers = await _context.Teachers
                .Include(t => t.Classes)
                .Select(t => new
                {
                    t.TeacherId,
                    t.FullName,
                    t.Email,
                    t.PhotoUrl,
                    t.Status,
                    ClassCount = t.Classes.Count
                })
                .ToListAsync();

            return Json(teachers);
        }

        // =====================================================
        // ➕ ADD TEACHER
        // =====================================================
        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] Teacher model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.FullName) || string.IsNullOrWhiteSpace(model.Email))
                return BadRequest("Invalid data.");

            var teacher = new Teacher
            {
                FullName = model.FullName,
                Email = model.Email,
                Status = model.Status ?? "Active",
                PhotoUrl = model.PhotoUrl
            };

            if (!string.IsNullOrWhiteSpace(model.PasswordHash))
                teacher.PasswordHash = HashPassword(model.PasswordHash);

            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        // =====================================================
        // ✏️ EDIT TEACHER
        // =====================================================
        [HttpPut("Edit"), HttpPost("Edit")] // supports both PUT and POST
        public async Task<IActionResult> Edit([FromBody] Teacher model)
        {
            var teacher = await _context.Teachers.FindAsync(model.TeacherId);
            if (teacher == null) return NotFound();

            teacher.FullName = model.FullName;
            teacher.Email = model.Email;
            teacher.Status = model.Status ?? teacher.Status;

            // Only update password if a new one is provided
            if (!string.IsNullOrWhiteSpace(model.PasswordHash))
                teacher.PasswordHash = HashPassword(model.PasswordHash);

            if (!string.IsNullOrWhiteSpace(model.PhotoUrl))
                teacher.PhotoUrl = model.PhotoUrl;

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // =====================================================
        // ❌ DELETE TEACHER
        // =====================================================
        [HttpDelete("Delete/{id}"), HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return NotFound();

            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        // =====================================================
        // 🔐 RESET PASSWORD
        // =====================================================
        [HttpPut("ResetPassword/{id}"), HttpPost("ResetPassword/{id}")]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] string newPassword)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return NotFound();

            if (string.IsNullOrWhiteSpace(newPassword))
                return BadRequest("Password cannot be empty.");

            teacher.PasswordHash = HashPassword(newPassword);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Password reset successfully." });
        }
    }
}
