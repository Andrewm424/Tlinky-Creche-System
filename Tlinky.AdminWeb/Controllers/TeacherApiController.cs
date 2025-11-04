using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Tlinky.AdminWeb.Data;
using Tlinky.AdminWeb.Models;

namespace Tlinky.AdminWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TeacherApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Utility: same SHA256 hashing used by admin side
        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        // ✅ Login Endpoint (used by Flutter)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { message = "Missing email or password." });

            try
            {
                // 🔒 Hash the incoming password for comparison
                var hashedInput = HashPassword(model.Password);

                var teacher = await _context.Teachers
                    .Include(t => t.Classes)
                    .FirstOrDefaultAsync(t =>
                        t.Email.ToLower() == model.Email.ToLower() &&
                        t.PasswordHash == hashedInput);

                if (teacher == null)
                    return Unauthorized(new { message = "Invalid credentials." });

                // 🧑‍🏫 Pick the first class if multiple
                var firstClass = teacher.Classes?.FirstOrDefault();
                var classId = firstClass?.ClassId;
                var className = firstClass?.Name ?? "Unassigned";

                return Ok(new
                {
                    teacherId = teacher.TeacherId,
                    fullName = teacher.FullName,
                    classId,
                    className,
                    email = teacher.Email
                });
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"❌ ERROR in TeacherApi login: {ex}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ✅ Optional: fetch teacher info by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTeacher(int id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.Classes)
                .FirstOrDefaultAsync(t => t.TeacherId == id);

            if (teacher == null)
                return NotFound(new { message = "Teacher not found." });

            var firstClass = teacher.Classes?.FirstOrDefault();
            var classId = firstClass?.ClassId;
            var className = firstClass?.Name ?? "Unassigned";

            return Ok(new
            {
                teacherId = teacher.TeacherId,
                fullName = teacher.FullName,
                classId,
                className,
                email = teacher.Email
            });
        }

        // ✅ DTO for incoming login request
        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
    }
}
