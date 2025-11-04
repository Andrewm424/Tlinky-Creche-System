using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tlinky.AdminWeb.Data;

// ✅ PDF + CSV libraries
using iTextSharp.text;
using iTextSharp.text.pdf;
using CsvHelper;

namespace Tlinky.AdminWeb.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // 📊 MAIN REPORTS DASHBOARD (Quick-Fix Version)
        // =========================================================
        public async Task<IActionResult> Index()
        {
            // ---------- ATTENDANCE ----------
            var attendanceRaw = await _context.Attendance
                .Include(a => a.Child).ThenInclude(c => c.Class)
                .AsNoTracking().ToListAsync();

            var attendanceSummary = attendanceRaw
                .Where(a => a.Child?.Class != null)
                .GroupBy(a => a.Child.Class.Name)
                .Select(g => new
                {
                    ClassName = g.Key,
                    Present = g.Count(x => x.Status.Equals("Present", StringComparison.OrdinalIgnoreCase)),
                    Absent = g.Count(x => x.Status.Equals("Absent", StringComparison.OrdinalIgnoreCase)),
                    Late = g.Count(x => x.Status.Equals("Late", StringComparison.OrdinalIgnoreCase))
                })
                .OrderBy(g => g.ClassName)
                .ToList();

            // ---------- INCIDENTS ----------
            var incidentRaw = await _context.Incidents
                .Include(i => i.Child).ThenInclude(c => c.Class)
                .AsNoTracking().ToListAsync();

            var incidentSummary = incidentRaw
                .OrderByDescending(i => i.Date)
                .Take(10)
                .Select(i => new
                {
                    Date = DateTime.SpecifyKind(i.Date, DateTimeKind.Utc).ToString("dd MMM"),
                    Description = i.Description ?? "(No details)",
                    ClassName = i.Child?.Class?.Name ?? "Unassigned"
                })
                .ToList();

            // ---------- PAYMENTS ----------
            var paymentsRaw = await _context.Payments.AsNoTracking().ToListAsync();

            var paymentsByMonth = paymentsRaw
                .GroupBy(p => string.IsNullOrEmpty(p.Month) ? "Unspecified" : p.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Paid = g.Count(x => x.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase)),
                    Total = g.Count(),
                    Percent = g.Count() == 0
                        ? 0
                        : (int)((g.Count(x => x.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase)) /
                                (double)g.Count()) * 100)
                })
                .OrderBy(g => g.Month)
                .ToList();

            // ---------- CHILDREN ----------
            var childrenRaw = await _context.Children
                .Include(c => c.Class).Include(c => c.Parent)
                .AsNoTracking().ToListAsync();

            var children = childrenRaw
                .Select(c => new
                {
                    c.FullName,
                    ClassName = c.Class != null ? c.Class.Name : "Unassigned",
                    Age = c.DOB.HasValue
                        ? (int)((DateTime.UtcNow - DateTime.SpecifyKind(c.DOB.Value, DateTimeKind.Utc)).TotalDays / 365.25)
                        : 0,
                    Guardian = c.Parent != null ? c.Parent.FullName : "Unknown"
                })
                .OrderBy(c => c.ClassName)
                .ToList();

            // ---------- PASS TO VIEW ----------
            ViewBag.AttendanceSummary = attendanceSummary;
            ViewBag.IncidentSummary = incidentSummary;
            ViewBag.PaymentsByMonth = paymentsByMonth;
            ViewBag.Children = children;

            return View();
        }

        // =========================================================
        // 🧾 EXPORT: Attendance → PDF
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> ExportAttendancePdf()
        {
            var records = await _context.Attendance
                .Include(a => a.Child).ThenInclude(c => c.Class)
                .AsNoTracking().ToListAsync();

            using var stream = new MemoryStream();
            var doc = new Document(PageSize.A4);
            PdfWriter.GetInstance(doc, stream);
            doc.Open();
            FontFactory.RegisterDirectories();

            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
            var tableFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);

            doc.Add(new Paragraph("Tlinky Crèche - Attendance Summary", titleFont));
            doc.Add(new Paragraph($"Generated on: {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC\n\n"));

            var table = new PdfPTable(3) { WidthPercentage = 100 };
            table.AddCell("Class");
            table.AddCell("Present");
            table.AddCell("Absent");

            var summary = records
                .Where(a => a.Child?.Class != null)
                .GroupBy(a => a.Child.Class.Name)
                .Select(g => new
                {
                    ClassName = g.Key,
                    Present = g.Count(x => x.Status.Equals("Present", StringComparison.OrdinalIgnoreCase)),
                    Absent = g.Count(x => x.Status.Equals("Absent", StringComparison.OrdinalIgnoreCase))
                });

            foreach (var item in summary)
            {
                table.AddCell(new Phrase(item.ClassName, tableFont));
                table.AddCell(new Phrase(item.Present.ToString(), tableFont));
                table.AddCell(new Phrase(item.Absent.ToString(), tableFont));
            }

            doc.Add(table);
            doc.Close();
            return File(stream.ToArray(), "application/pdf", "AttendanceReport.pdf");
        }

        // =========================================================
        // 🧾 EXPORT: Incidents → PDF
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> ExportIncidentsPdf()
        {
            try
            {
                // ✅ Step 1: Load all data in memory first
                var incidentRaw = await _context.Incidents
                    .Include(i => i.Child).ThenInclude(c => c.Class)
                    .AsNoTracking()
                    .ToListAsync();

                // ✅ Step 2: Disconnect DB before building PDF
                var incidents = incidentRaw
                    .OrderByDescending(i => i.Date)
                    .Take(100) // safety limit (adjust if you have thousands)
                    .Select(i => new
                    {
                        Date = i.Date != default
                            ? DateTime.SpecifyKind(i.Date, DateTimeKind.Utc).ToString("dd MMM yyyy")
                            : "N/A",
                        Description = i.Description ?? "(No details)",
                        ClassName = i.Child?.Class?.Name ?? "Unassigned"
                    })
                    .ToList();

                // ✅ Step 3: Generate PDF safely
                using var stream = new MemoryStream();
                var doc = new Document(PageSize.A4);
                PdfWriter.GetInstance(doc, stream);
                doc.Open();
                FontFactory.RegisterDirectories();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                var bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);

                doc.Add(new Paragraph("Tlinky Crèche - Incident Report Summary", titleFont));
                doc.Add(new Paragraph($"Generated: {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC\n\n"));

                foreach (var i in incidents)
                {
                    doc.Add(new Paragraph($"{i.Date} - {i.Description} ({i.ClassName})", bodyFont));
                }

                doc.Close();

                return File(stream.ToArray(), "application/pdf", "IncidentReports.pdf");
            }
            catch (Exception ex)
            {
                // ✅ Graceful fallback if the DB drops mid-export
                Console.WriteLine("⚠️ Incident export failed: " + ex.Message);
                return BadRequest("Could not generate Incident PDF. Try again in a few seconds.");
            }
        }


        // =========================================================
        // 📈 EXPORT: Fee Compliance → CSV
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> ExportFeeCsv()
        {
            var payments = await _context.Payments.AsNoTracking().ToListAsync();

            var compliance = payments
                .GroupBy(p => string.IsNullOrEmpty(p.Month) ? "Unspecified" : p.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Paid = g.Count(x => x.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase)),
                    Total = g.Count(),
                    Percent = g.Count() == 0
                        ? 0
                        : (int)((g.Count(x => x.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase)) /
                                (double)g.Count()) * 100)
                })
                .OrderBy(g => g.Month)
                .ToList();

            using var mem = new MemoryStream();
            using var writer = new StreamWriter(mem, Encoding.UTF8);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(compliance);
            writer.Flush();

            return File(mem.ToArray(), "text/csv", "FeeCompliance.csv");
        }

        // =========================================================
        // 🧾 EXPORT: Children Roster → PDF
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> ExportChildrenPdf()
        {
            var children = await _context.Children
                .Include(c => c.Class).Include(c => c.Parent)
                .AsNoTracking().ToListAsync();

            using var stream = new MemoryStream();
            var doc = new Document(PageSize.A4.Rotate());
            PdfWriter.GetInstance(doc, stream);
            doc.Open();
            FontFactory.RegisterDirectories();

            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
            var tableFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);

            doc.Add(new Paragraph("Tlinky Crèche - Children Roster", titleFont));
            doc.Add(new Paragraph($"Generated on: {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC\n\n"));

            var table = new PdfPTable(4) { WidthPercentage = 100 };
            table.AddCell("Name");
            table.AddCell("Class");
            table.AddCell("Age");
            table.AddCell("Guardian");

            foreach (var c in children)
            {
                var age = c.DOB.HasValue
                    ? (int)((DateTime.UtcNow - DateTime.SpecifyKind(c.DOB.Value, DateTimeKind.Utc)).TotalDays / 365.25)
                    : 0;

                table.AddCell(new Phrase(c.FullName, tableFont));
                table.AddCell(new Phrase(c.Class?.Name ?? "Unassigned", tableFont));
                table.AddCell(new Phrase(age.ToString(), tableFont));
                table.AddCell(new Phrase(c.Parent?.FullName ?? "Unknown", tableFont));
            }

            doc.Add(table);
            doc.Close();

            return File(stream.ToArray(), "application/pdf", "ChildrenRoster.pdf");
        }
    }
}
