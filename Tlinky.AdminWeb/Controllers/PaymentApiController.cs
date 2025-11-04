using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tlinky.AdminWeb.Data;
using Tlinky.AdminWeb.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Tlinky.AdminWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly Cloudinary _cloudinary;

        public PaymentApiController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;

            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
        }

        // ✅ GET: /api/PaymentApi/GetByParent/5
        [HttpGet("GetByParent/{parentId}")]
        public async Task<IActionResult> GetByParent(int parentId)
        {
            try
            {
                var parent = await _context.Parents
                    .Include(p => p.Children)
                    .FirstOrDefaultAsync(p => p.ParentId == parentId);

                if (parent == null)
                    return NotFound(new { success = false, message = "Parent not found." });

                var payments = await _context.Payments
                    .Include(p => p.Child)
                    .Where(p => p.ParentId == parentId)
                    .OrderByDescending(p => p.DateUploaded)
                    .Select(p => new
                    {
                        p.PaymentId,
                        p.Month,
                        p.Amount,
                        p.Status,
                        p.ProofUrl,
                        ChildName = p.Child != null ? p.Child.FullName : "N/A",
                        Uploaded = p.DateUploaded.ToString("yyyy-MM-dd HH:mm")
                    })
                    .ToListAsync();

                decimal totalFees = payments.Sum(p => p.Amount);
                decimal totalPaid = await _context.Payments
                    .Where(p => p.ParentId == parentId && p.Status == "Approved")
                    .SumAsync(p => p.Amount);

                decimal balance = totalFees - totalPaid;

                return Ok(new
                {
                    success = true,
                    balance,
                    totalFees,
                    totalPaid,
                    payments
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in GetByParent: {ex}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ✅ POST: /api/PaymentApi/UploadProof
        [HttpPost("UploadProof")]
        public async Task<IActionResult> UploadProof([FromForm] int paymentId, IFormFile file)
        {
            if (file == null || file.Length == 0 || paymentId <= 0)
                return BadRequest(new { success = false, message = "Missing file or payment ID." });

            try
            {
                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "tlinky/payments"
                };
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.Error != null)
                    return BadRequest(new { success = false, message = uploadResult.Error.Message });

                var payment = await _context.Payments
                    .Include(p => p.Parent)
                    .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

                if (payment == null)
                    return NotFound(new { success = false, message = "Payment not found." });

                payment.ProofUrl = uploadResult.SecureUrl?.AbsoluteUri ?? string.Empty;
                payment.Status = "Pending";
                // DateUploaded is mapped as "timestamp with time zone" in your model – UTC is OK here:
                payment.DateUploaded = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var parentName = payment.Parent?.FullName ?? "Parent";
                var amount = payment.Amount.ToString("0.00");

                _context.Notifications.Add(new Notification
                {
                    Type = "Payment",
                    Message = $"Payment proof uploaded: R{amount} from {parentName}",
                    // 👇 IMPORTANT: save as Unspecified to match "timestamp without time zone"
                    DateCreated = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)
                });

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Proof uploaded successfully",
                    paymentId = payment.PaymentId,
                    status = payment.Status,
                    proofUrl = payment.ProofUrl
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR uploading proof: {ex}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

    }
}

