// Controllers/AccountController.cs
using Microsoft.AspNetCore.Mvc;

public class AccountController : Controller
{
    public IActionResult Login()
    {
        return View(); // returns Login.cshtml
    }

    public IActionResult Logout()
    {
        // Clear authentication, session, cookies etc.
        HttpContext.Session.Clear();
        return RedirectToAction("Login", "Account");
    }
}
