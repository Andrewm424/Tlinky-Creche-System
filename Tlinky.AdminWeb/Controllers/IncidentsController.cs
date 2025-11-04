using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tlinky.AdminWeb.Data;
using Tlinky.AdminWeb.Models;
using System.Threading.Tasks;

namespace Tlinky.AdminWeb.Controllers
{
    public class IncidentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IncidentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ List all incidents
        public async Task<IActionResult> Index()
        {
            var incidents = await _context.Incidents
                .Include(i => i.Child)
                .Include(i => i.Teacher)
                .Include(i => i.Class)
                .OrderByDescending(i => i.Date)
                .ToListAsync();
            return View(incidents);
        }

        // ✅ View details
        public async Task<IActionResult> Details(int id)
        {
            var incident = await _context.Incidents
                .Include(i => i.Child)
                .Include(i => i.Teacher)
                .Include(i => i.Class)
                .FirstOrDefaultAsync(i => i.IncidentId == id);

            if (incident == null) return NotFound();
            return View(incident);
        }
    }
}
