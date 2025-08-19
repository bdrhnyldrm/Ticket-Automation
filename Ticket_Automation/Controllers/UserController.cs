using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Ticket_Automation.Entity;
using Ticket_Automation.ViewModels.User;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Ticket_Automation.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly DatabaseContext _databaseContext;
        public UserController(DatabaseContext databaseContext)
            => _databaseContext = databaseContext;

        // Şifre Hashleme Metodu
        private string HashPassword(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return $"{Convert.ToBase64String(salt)}.{hashed}";
        }

        // Her iki formda da kullanılacak rol listesini doldurur
        private void PopulateRoles()
        {
            ViewBag.Roles = Enum.GetValues(typeof(UserRole))
                .Cast<UserRole>()
                .Select(r => new SelectListItem
                {
                    Value = ((int)r).ToString(),
                    Text = r.ToString()
                })
                .ToList();
        }

        // GET: /User
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _databaseContext.Users.ToListAsync();
            return View(users);
        }

        // GET: /User/Detail/5
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var user = await _databaseContext.Users
                .Include(u => u.CreatedTickets)
                .Include(u => u.AssignedTickets)
                .Include(u => u.TicketMessages)
                .Include(u => u.Notifications)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();
            return View(user);
        }

        // POST: /User/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _databaseContext.Users
                .Include(u => u.CreatedTickets)
                .Include(u => u.AssignedTickets)
                .Include(u => u.TicketMessages)
                .Include(u => u.Notifications)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            _databaseContext.Users.Remove(user);
            await _databaseContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Kullanıcı silindi.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /User/Create
        [HttpGet]
        public IActionResult Create()
        {
            PopulateRoles();
            return View(new UserCreateViewModel());
        }

        // POST: /User/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PopulateRoles();
                return View(model);
            }

            var newUser = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Password = HashPassword(model.Password), // Şifreyi hashledik ✔️
                PhoneNumber = model.PhoneNumber,
                Role = model.Role
            };

            _databaseContext.Users.Add(newUser);
            await _databaseContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Kullanıcı başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /User/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _databaseContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            PopulateRoles();
            var vm = new UserEditViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role
            };
            return View(vm);
        }

        // POST: /User/Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PopulateRoles();
                return View(model);
            }

            var user = await _databaseContext.Users.FirstOrDefaultAsync(u => u.Id == model.Id);
            if (user == null) return NotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.Role = model.Role;

            await _databaseContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Kullanıcı ve rol başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
