using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Tlinky.AdminWeb.Data;
using Tlinky.AdminWeb.Models;

namespace Tlinky.AdminWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ParentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ParentsController(ApplicationDbContext context) => _context = context;

        // ✅ Main page
        [HttpGet]
        public IActionResult Index() => View();

        // ✅ Get all parents (used by JS loadParents)
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var parents = await _context.Parents
                .Include(p => p.Children)
                .OrderBy(p => p.FullName)
                .Select(p => new
                {
                    p.ParentId,
                    p.FullName,
                    p.Email,
                    p.Phone,
                    p.Status,
                    Children = p.Children.Any()
                        ? string.Join(", ", p.Children.Select(c => c.FullName))
                        : "None"
                })
                .ToListAsync();

            return Json(parents);
        }

        // ✅ Get unlinked children (for dropdown)
        [HttpGet("Dropdowns")]
        public async Task<IActionResult> Dropdowns()
        {
            var children = await _context.Children
                .Where(c => c.ParentId == null && c.Status == "Active")
                .Select(c => new { c.ChildId, c.FullName })
                .OrderBy(c => c.FullName)
                .ToListAsync();

            return Json(new { children });
        }

        // ✅ Add parent + link children
        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] ParentCreateDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid data.");

            var parent = new Parent
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                PasswordHash = HashPassword(dto.PasswordHash),
                Status = "Active"
            };

            _context.Parents.Add(parent);
            await _context.SaveChangesAsync();

            if (dto.ChildIds != null && dto.ChildIds.Any())
            {
                var children = await _context.Children
                    .Where(c => dto.ChildIds.Contains(c.ChildId))
                    .ToListAsync();

                foreach (var child in children)
                    child.ParentId = parent.ParentId;

                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Parent added successfully." });
        }

        // ✅ Edit parent
        [HttpPut("Edit")]
        public async Task<IActionResult> Edit([FromBody] ParentUpdateDto dto)
        {
            var existing = await _context.Parents
                .Include(p => p.Children)
                .FirstOrDefaultAsync(p => p.ParentId == dto.ParentId);

            if (existing == null)
                return NotFound();

            existing.FullName = dto.FullName;
            existing.Email = dto.Email;
            existing.Phone = dto.Phone;
            existing.Status = dto.Status;

            if (!string.IsNullOrWhiteSpace(dto.PasswordHash))
                existing.PasswordHash = HashPassword(dto.PasswordHash);

            // unlink old children
            var oldChildren = await _context.Children
                .Where(c => c.ParentId == dto.ParentId)
                .ToListAsync();
            foreach (var c in oldChildren)
                c.ParentId = null;

            // link new children
            if (dto.ChildIds != null && dto.ChildIds.Any())
            {
                var newChildren = await _context.Children
                    .Where(c => dto.ChildIds.Contains(c.ChildId))
                    .ToListAsync();
                foreach (var c in newChildren)
                    c.ParentId = existing.ParentId;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Parent updated successfully." });
        }

        // ✅ Delete parent
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var parent = await _context.Parents
                .Include(p => p.Children)
                .FirstOrDefaultAsync(p => p.ParentId == id);

            if (parent == null)
                return NotFound();

            foreach (var c in parent.Children ?? [])
                c.ParentId = null;

            _context.Parents.Remove(parent);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Parent deleted successfully." });
        }

        // ✅ Reset password
        [HttpPut("ResetPassword/{id}")]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] string newPassword)
        {
            var parent = await _context.Parents.FindAsync(id);
            if (parent == null) return NotFound();

            parent.PasswordHash = HashPassword(newPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password reset successfully." });
        }

        // 🔐 Hash helper
        private static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }

    // ✅ DTOs
    public class ParentCreateDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public List<int>? ChildIds { get; set; }
    }

    public class ParentUpdateDto : ParentCreateDto
    {
        public int ParentId { get; set; }
        public string Status { get; set; } = "Active";
    }
}
