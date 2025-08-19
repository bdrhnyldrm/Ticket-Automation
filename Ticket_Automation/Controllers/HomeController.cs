using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ticket_Automation.Entity;
using Ticket_Automation.Models;

namespace Ticket_Automation.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly DatabaseContext _databaseContext;

        public HomeController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Kullanýcý ID'sini Cookie'den (Claims) alýyoruz
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _databaseContext.Users
                .Include(u => u.CreatedTickets)
                .Include(u => u.AssignedTickets)
                .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null)
                return RedirectToAction("Login", "Account");

            // Admin için tüm istatistikler
            if (User.IsInRole("Admin"))
            {
                ViewBag.TotalUsers = _databaseContext.Users.Count();
                ViewBag.AdminUsers = _databaseContext.Users.Count(u => u.Role == UserRole.Admin);
                ViewBag.PersonelUsers = _databaseContext.Users.Count(u => u.Role == UserRole.Personel);
                ViewBag.CustomerUsers = _databaseContext.Users.Count(u => u.Role == UserRole.Customer);

                ViewBag.TotalTickets = _databaseContext.Tickets.Count();
                ViewBag.ActiveTickets = _databaseContext.Tickets.Count(t => t.Status == TicketStatus.Open);
                ViewBag.SolvedTickets = _databaseContext.Tickets.Count(t => t.Status == TicketStatus.Solved);
                ViewBag.ClosedTickets = _databaseContext.Tickets.Count(t => t.Status == TicketStatus.Closed);
            }

            // Customer için destek talepleri
            if (User.IsInRole("Customer"))
            {
                ViewBag.CustomerTickets = _databaseContext.Tickets
                    .Where(t => t.CreatedById == user.Id)
                    .ToList();
            }

            return View(user);
        }

    }
}

