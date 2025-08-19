using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Ticket_Automation.Entity;

namespace Ticket_Automation.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly DatabaseContext _databaseContext;
        public NotificationController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        // GET: /Notification/List - Kullanıcıya Ait Bildirimler
        public async Task<IActionResult> List()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var notifications = await _databaseContext.Notifications
                .Include(n => n.Ticket)
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notifications);
        }

        // Bildirim okundu olarak işaretle ve ticket detayına yönlendir
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            var notification = await _databaseContext.Notifications.FindAsync(notificationId);

            if (notification == null)
                return NotFound();

            notification.IsRead = true;
            await _databaseContext.SaveChangesAsync();

            return RedirectToAction("Detail", "Ticket", new { id = notification.TicketId });
        }
    }
}
