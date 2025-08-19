using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;
using Ticket_Automation.Entity;
using Ticket_Automation.ViewModels.Ticket;

namespace Ticket_Automation.Controllers
{

    [Authorize]
    public class TicketController : Controller
    {
        private readonly DatabaseContext _databaseContext;
        public TicketController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        // GET: Ticket/List - Tüm Ticket'ları Listele
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            IQueryable<Ticket> tickets = _databaseContext.Tickets
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo);

            if (userRole == "Customer")
            {
                // Müşteri sadece kendi Ticket'larını görür
                tickets = tickets.Where(t => t.CreatedById == userId);
            }
            else if (userRole == "Personel")
            {
                // Personel sadece kendisine atanan Ticket'ları görür
                tickets = tickets.Where(t => t.AssignedToId == userId);
            }

            var ticketList = await tickets.ToListAsync();
            return View(ticketList);
        }

        // GET: /Ticket/Create - Yeni Ticket Formu
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string? filePath = null;

            if (model.Attachment != null && model.Attachment.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(model.Attachment.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "attachments");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fullPath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.Attachment.CopyToAsync(stream);
                }

                filePath = "/attachments/" + fileName;
            }

            var ticket = new Ticket
            {
                Title = model.Title,
                Description = model.Description,
                Priority = model.Priority,
                Status = TicketStatus.Open,
                CreatedAt = DateTime.UtcNow,
                CreatedById = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value),
                AttachmentPath = filePath
            };

            _databaseContext.Tickets.Add(ticket);
            await _databaseContext.SaveChangesAsync();

            // ✅ Adminlere Bildirim Gönder
            var adminUsers = await _databaseContext.Users
                .Where(u => u.Role == UserRole.Admin)
                .ToListAsync();

            foreach (var admin in adminUsers)
            {
                _databaseContext.Notifications.Add(new Notification
                {
                    UserId = admin.Id,
                    NotificationType = NotificationType.NewTicket,
                    TicketId = ticket.Id,
                    CreatedAt = DateTime.Now
                });
            }

            await _databaseContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Destek talebi başarıyla oluşturuldu.";
            return RedirectToAction(nameof(List));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Assign(int ticketId, int assignedToId)
        {
            var ticket = await _databaseContext.Tickets.FindAsync(ticketId);
            if (ticket == null)
                return NotFound();

            var personel = await _databaseContext.Users.FirstOrDefaultAsync(u => u.Id == assignedToId && u.Role == UserRole.Personel);
            if (personel == null)
                return NotFound("Atanacak personel bulunamadı.");

            // Ticket'ı Personel'e Ata
            ticket.AssignedToId = personel.Id;
            await _databaseContext.SaveChangesAsync();

            // Bildirim Gönderilecek Kişiler (Personel ve Ticket Oluşturan Müşteri)
            var notificationUsers = new List<int> { personel.Id, ticket.CreatedById };

            foreach (var id in notificationUsers.Distinct())
            {
                _databaseContext.Notifications.Add(new Notification
                {
                    UserId = id,
                    NotificationType = NotificationType.TicketAssigned,
                    TicketId = ticket.Id,
                    CreatedAt = DateTime.Now
                });
            }

            await _databaseContext.SaveChangesAsync();

            return RedirectToAction("Detail", new { id = ticketId });
        }




        // GET: /Ticket/Detail/5 - Ticket Detay Görüntüle
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var ticket = await _databaseContext.Tickets
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Include(t => t.Messages)
                    .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
                return NotFound();

            // Eğer kullanıcı adminse, personel listesi gönder
            if (User.IsInRole("Admin"))
            {
                var personelList = _databaseContext.Users
                    .Where(u => u.Role == UserRole.Personel)
                    .ToList();

                ViewBag.PersonelList = personelList;
            }

            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMessage(int ticketId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Mesaj içeriği boş olamaz.";
                return RedirectToAction("Detail", new { id = ticketId });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var ticket = await _databaseContext.Tickets
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null)
                return NotFound();

            var message = new TicketMessage
            {
                TicketId = ticketId,
                SenderId = userId,
                Content = content,
                SentAt = DateTime.Now
            };

            _databaseContext.TicketMessages.Add(message);
            await _databaseContext.SaveChangesAsync();

            // Bildirim Gönder: Ticket Sahibi ve Atanan Kişi
            var notificationUsers = new List<int>();
            if (ticket.CreatedById != userId) notificationUsers.Add(ticket.CreatedById);
            if (ticket.AssignedToId.HasValue && ticket.AssignedToId != userId)
                notificationUsers.Add(ticket.AssignedToId.Value);

            // ✅ Adminler için bildirim gönder
            var adminUsers = await _databaseContext.Users
                .Where(u => u.Role == UserRole.Admin)
                .ToListAsync();

            foreach (var admin in adminUsers)
            {
                if (admin.Id != userId) // Kendi kendine bildirim gitmesin
                {
                    notificationUsers.Add(admin.Id);
                }
            }

            foreach (var id in notificationUsers.Distinct())
            {
                _databaseContext.Notifications.Add(new Notification
                {
                    UserId = id,
                    NotificationType = NotificationType.NewMessage,
                    TicketId = ticketId,
                    CreatedAt = DateTime.Now
                });
            }

            await _databaseContext.SaveChangesAsync();
            return RedirectToAction("Detail", new { id = ticketId });
        }


        // POST: /Ticket/UpdateStatus - Durum Güncelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, TicketStatus status)
        {
            var ticket = await _databaseContext.Tickets
                .Include(t => t.CreatedBy)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
                return NotFound();

            // Ticket Durumunu Güncelle
            ticket.Status = status;
            if (status == TicketStatus.Closed)
                ticket.ClosedAt = DateTime.UtcNow;

            await _databaseContext.SaveChangesAsync();

            // Bildirim Gönder: Ticket Sahibi (Sorun Çözüldü)
            if (status == TicketStatus.Solved)
            {
                _databaseContext.Notifications.Add(new Notification
                {
                    UserId = ticket.CreatedById,
                    NotificationType = NotificationType.TicketUpdated,
                    TicketId = ticket.Id,
                    CreatedAt = DateTime.Now
                });

                await _databaseContext.SaveChangesAsync();
            }

            return RedirectToAction("Detail", new { id });
        }


    }
}
