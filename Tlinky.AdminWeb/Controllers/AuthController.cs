using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Tlinky.AdminWeb.Data;
using Tlinky.AdminWeb.Models;

namespace Tlinky.AdminWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ✅ Admin Login (for MVC or testing)
        [HttpPost("LoginAdmin")]
        public async Task<IActionResult> LoginAdmin([FromBody] LoginRequest request)
        {
            var admin = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (admin == null || !BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash))
                return Unauthorized(new { success = false, message = "Invalid admin credentials." });

            var token = GenerateJwtToken(admin.UserId, admin.Email, "Admin");
            return Ok(new { success = true, role = "Admin", email = admin.Email, token });
        }

        // ✅ Teacher Login (for Flutter)
        [HttpPost("LoginTeacher")]
        public async Task<IActionResult> LoginTeacher([FromBody] LoginRequest request)
        {
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Email == request.Email);

            if (teacher == null || !BCrypt.Net.BCrypt.Verify(request.Password, teacher.PasswordHash))
                return Unauthorized(new { success = false, message = "Invalid teacher credentials." });

            var token = GenerateJwtToken(teacher.TeacherId, teacher.Email, "Teacher");

            return Ok(new
            {
                success = true,
                role = "Teacher",
                teacherId = teacher.TeacherId,
                fullName = teacher.FullName,
                email = teacher.Email,
                token
            });
        }

        // ✅ Parent Login (for Flutter)
        [HttpPost("LoginParent")]
        public async Task<IActionResult> LoginParent([FromBody] LoginRequest request)
        {
            var parent = await _context.Parents
                .FirstOrDefaultAsync(p => p.Email == request.Email);

            if (parent == null || !BCrypt.Net.BCrypt.Verify(request.Password, parent.PasswordHash))
                return Unauthorized(new { success = false, message = "Invalid parent credentials." });

            var token = GenerateJwtToken(parent.ParentId, parent.Email, "Parent");

            return Ok(new
            {
                success = true,
                role = "Parent",
                parentId = parent.ParentId,
                fullName = parent.FullName,
                email = parent.Email,
                token
            });
        }

        // 🔒 Token generator
        private string GenerateJwtToken(int id, string email, string role)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "default_secret_key"));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ✅ Optional endpoint for testing tokens
        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new { id, email, role });
        }
    }

    // ✅ Reusable login DTO
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
