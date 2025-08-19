using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ticket_Automation.Entity;
using Ticket_Automation.ViewModels.Reports;

namespace Ticket_Automation.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly DatabaseContext _context;

        public ReportsController(DatabaseContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var totalTickets = await _context.Tickets.CountAsync();
            var openTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Open);
            var solvedTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Solved);

            var topPersonnel = await _context.Users
                .Where(u => u.Role == UserRole.Personel)
                .Select(u => new TopPersonnelReportViewModel
                {
                    Name = u.FirstName + " " + u.LastName,
                    AssignedCount = u.AssignedTickets.Count()
                })
                .OrderByDescending(u => u.AssignedCount)
                .Take(5)
                .ToListAsync();

            var model = new ReportViewModel
            {
                TotalTickets = totalTickets,
                OpenTickets = openTickets,
                SolvedTickets = solvedTickets,
                TopPersonnel = topPersonnel
            };

            return View(model);
        }
        public async Task<IActionResult> DownloadTopPersonnelReport()
        {
            var topPersonnel = await _context.Users
                .Where(u => u.Role == UserRole.Personel)
                .Select(u => new
                {
                    Name = u.FirstName + " " + u.LastName,
                    Tickets = u.AssignedTickets.Select(t => new
                    {
                        Title = t.Title,
                        Status = t.Status,
                        Priority = t.Priority,
                        CreatedAt = t.CreatedAt
                    }).ToList()
                })
                .OrderByDescending(u => u.Tickets.Count)
                .Take(5)
                .ToListAsync();

            using var memoryStream = new MemoryStream();

            Document document = new Document(PageSize.A4, 40, 40, 40, 40);
            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
            writer.CloseStream = false;

            document.Open();

            // ✔️ Türkçe karakter desteği
            string fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "arial.ttf");
            BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            Font titleFont = new Font(baseFont, 20, Font.BOLD, BaseColor.Black);
            Font headerFont = new Font(baseFont, 12, Font.BOLD, BaseColor.White);
            Font cellFont = new Font(baseFont, 12, Font.NORMAL, BaseColor.Black);
            Font sectionFont = new Font(baseFont, 14, Font.BOLD, new BaseColor(33, 150, 243)); // Mavi

            // Başlık
            Paragraph title = new Paragraph("📊 En Çok Atama Yapılan Personeller Raporu", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20f
            };
            document.Add(title);

            foreach (var personel in topPersonnel)
            {
                Paragraph personelTitle = new Paragraph($"{personel.Name} - {personel.Tickets.Count} Atanan Ticket", sectionFont)
                {
                    SpacingAfter = 10f,
                    SpacingBefore = 10f
                };
                document.Add(personelTitle);

                PdfPTable table = new PdfPTable(4) { WidthPercentage = 100 };
                table.SetWidths(new float[] { 3f, 2f, 2f, 3f });

                // Başlıklar
                AddCellToTable(table, "Başlık", headerFont, new BaseColor(52, 58, 64)); // Gri
                AddCellToTable(table, "Durum", headerFont, new BaseColor(52, 58, 64));
                AddCellToTable(table, "Öncelik", headerFont, new BaseColor(52, 58, 64));
                AddCellToTable(table, "Oluşturulma Tarihi", headerFont, new BaseColor(52, 58, 64));

                foreach (var ticket in personel.Tickets)
                {
                    AddCellToTable(table, ticket.Title, cellFont, BaseColor.White);
                    AddCellToTable(table, ticket.Status.ToString(), cellFont, BaseColor.White);
                    AddCellToTable(table, ticket.Priority.ToString(), cellFont, BaseColor.White);
                    AddCellToTable(table, ticket.CreatedAt.ToString("dd.MM.yyyy HH:mm"), cellFont, BaseColor.White);
                }

                document.Add(table);
            }

            document.Close();

            memoryStream.Position = 0;
            return File(memoryStream.ToArray(), "application/pdf", "TopPersonnelReport.pdf");
        }

        private void AddCellToTable(PdfPTable table, string text, Font font, BaseColor backgroundColor)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font))
            {
                BackgroundColor = backgroundColor,
                Padding = 5,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            table.AddCell(cell);
        }
    }
}
