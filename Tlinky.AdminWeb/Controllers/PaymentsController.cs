using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tlinky.AdminWeb.Data;
using Tlinky.AdminWeb.Models;
using Tlinky.AdminWeb.Helpers;

namespace Tlinky.AdminWeb.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public PaymentsController(ApplicationDbContext context) => _context = context;

        public IActionResult Index() => View();

        // ✅ Get payments for selected month
        [HttpGet("Payments/ByMonth/{month?}")]
        public async Task<IActionResult> GetByMonth(string? month)
        {
            month ??= DateTime.Now.ToString("MMMM yyyy");

            var payments = await _context.Payments
                .Include(p => p.Parent)
                .Include(p => p.Child)
                .Where(p => p.Month == month)
                .OrderBy(p => p.Parent.FullName)
                .Select(p => new
                {
                    p.PaymentId,
                    p.Month,
                    p.Amount,
                    p.Status,
                    p.ProofUrl,
                    DateUploaded = p.DateUploaded.ToString("yyyy-MM-dd HH:mm"),
                    ParentName = p.Parent != null ? p.Parent.FullName : "N/A",
                    ChildName = p.Child != null ? p.Child.FullName : "N/A"
                })
                .ToListAsync();

            return Json(payments);
        }

        // ✅ Generate all payments for selected month (manual trigger)
        [HttpPost("Payments/Generate/{month?}")]
        public async Task<IActionResult> Generate(string? month)
        {
            try
            {
                month ??= DateTime.Now.ToString("MMMM yyyy");

                // Load settings
                var setting = await _context.Settings.FirstOrDefaultAsync() ?? new SystemSetting();

                // Load all active children linked to active parents
                var children = await _context.Children
                    .Include(c => c.Parent)
                    .Where(c => c.Status == "Active" && c.Parent != null && c.Parent.Status == "Active")
                    .ToListAsync();

                if (!children.Any())
                    return BadRequest(new { error = "No active children found." });

                // Remove existing payments for this month (optional)
                var existing = await _context.Payments
                    .Where(p => p.Month == month)
                    .ToListAsync();

                _context.Payments.RemoveRange(existing);

                // Create new payments
                foreach (var child in children)
                {
                    var amount = FeeCalculator.CalculateMonthlyFee(child, setting);
                    _context.Payments.Add(new Payment
                    {
                        ParentId = child.ParentId,
                        ChildId = child.ChildId,
                        Month = month,
                        Amount = amount,
                        Status = "Pending",
                        // 👇 Ensure this is stored as a neutral timestamp (not UTC)
                        DateUploaded = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = $"Payments generated for {month}", count = children.Count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // ✅ Update payment status
        [HttpPut("Payments/UpdateStatus/{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] Payment update)
        {
            var existing = await _context.Payments.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Status = update.Status;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Payment status updated" });
        }

        // ✅ Attach proof of payment (from mobile upload)
        [HttpPut("Payments/AttachProof/{id}")]
        public async Task<IActionResult> AttachProof(int id, [FromBody] string proofUrl)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return NotFound(new { message = "Payment not found" });

            payment.ProofUrl = proofUrl;
            payment.Status = "Pending";
            // 👇 Fix timezone issue (use DateTimeKind.Unspecified)
            payment.DateUploaded = DateTime.UtcNow
;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Proof uploaded successfully",
                proofUrl
            });
        }

        // ✅ Delete payment
        [HttpDelete("Payments/Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Payment deleted successfully" });
        }
    }
}
