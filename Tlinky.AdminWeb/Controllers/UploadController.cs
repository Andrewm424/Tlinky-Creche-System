using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;

namespace Tlinky.AdminWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly Cloudinary _cloudinary;

        public UploadController(IConfiguration config)
        {
            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
        }

        [HttpPost("teacher-photo")]
        public async Task<IActionResult> UploadTeacherPhoto(IFormFile file) =>
            await UploadImage(file, "tlinky/teachers");

        [HttpPost("child-photo")]
        public async Task<IActionResult> UploadChildPhoto(IFormFile file) =>
            await UploadImage(file, "tlinky/children");

        [HttpPost("payment-proof")]
        public async Task<IActionResult> UploadPaymentProof(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "tlinky/payments"
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
                return BadRequest(uploadResult.Error.Message);

            return Ok(new { url = uploadResult.SecureUrl.AbsoluteUri });
        }


        private async Task<IActionResult> UploadImage(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };
            var upload = await _cloudinary.UploadAsync(uploadParams);
            if (upload.Error != null) return BadRequest(upload.Error.Message);

            return Ok(new { url = upload.SecureUrl.AbsoluteUri });
        }
    }
}
