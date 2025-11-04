using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tlinky.AdminWeb.Data;
using Tlinky.AdminWeb.Models;

namespace Tlinky.AdminWeb.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ GET: /Login
        [HttpGet]
        public IActionResult Index()
        {
            // If already logged in, redirect to Dashboard
            if (HttpContext.Session.GetString("AdminEmail") != null)
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        // ✅ POST: /Login/Authenticate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Authenticate(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter both email and password.";
                return View("Index");
            }

            try
            {
                // 🧠 Find admin by email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    ViewBag.Error = "Invalid email or password.";
                    return View("Index");
                }

                // ✅ Create secure session
                HttpContext.Session.SetString("AdminEmail", user.Email);
                HttpContext.Session.SetString("AdminRole", user.Role);

                // ✅ Redirect to dashboard or admin home
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ LOGIN ERROR: {ex.Message}");
                ViewBag.Error = "An unexpected error occurred while logging in.";
                return View("Index");
            }
        }

        // ✅ Logout and clear session
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index"); // Back to /Login
        }
    }
}
