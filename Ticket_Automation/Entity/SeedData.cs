using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace Ticket_Automation.Entity
{
    public static class SeedData
    {
        public static void TestVerileriniDoldur(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }

            if (!context.Users.Any())
            {
                var users = new List<User>
                {
                    new User
                    {
                        FirstName = "Admin",
                        LastName = "User",
                        Email = "admin@example.com",
                        Password = HashPassword("123456"),
                        PhoneNumber = "5551112233",
                        Role = UserRole.Admin,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        FirstName = "Ali",
                        LastName = "Yılmaz",
                        Email = "ali@example.com",
                        Password = HashPassword("123456"),
                        PhoneNumber = "5552223344",
                        Role = UserRole.Personel,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        FirstName = "Ayşe",
                        LastName = "Kara",
                        Email = "ayse@example.com",
                        Password = HashPassword("123456"),
                        PhoneNumber = "5553334455",
                        Role = UserRole.Customer,
                        CreatedAt = DateTime.UtcNow
                    }
                };
                context.Users.AddRange(users);
                context.SaveChanges();
            }

            if (!context.Tickets.Any())
            {
                var tickets = new List<Ticket>
                {
                    new Ticket
                    {
                        Title = "Şifremi Sıfırlayamıyorum",
                        Description = "Şifre sıfırlama emaili gelmiyor.",
                        CreatedById = 3,
                        AssignedToId = 2,
                        Priority = TicketPriority.Normal,
                        Status = TicketStatus.Open,
                        CreatedAt = DateTime.UtcNow,
                        AttachmentPath = "/attachments/1.png" // ✔️ Dosya Yolu Eklendi
                    }
                };
                context.Tickets.AddRange(tickets);
                context.SaveChanges();
            }

            if (!context.TicketMessages.Any())
            {
                var messages = new List<TicketMessage>
                {
                    new TicketMessage
                    {
                        TicketId = 1,
                        SenderId = 3,
                        Content = "Şifremi sıfırlamak istiyorum ama email gelmedi.",
                        SentAt = DateTime.UtcNow
                    },
                    new TicketMessage
                    {
                        TicketId = 1,
                        SenderId = 2,
                        Content = "Merhaba Ayşe Hanım, hemen yardımcı oluyorum.",
                        SentAt = DateTime.UtcNow
                    }
                };
                context.TicketMessages.AddRange(messages);
                context.SaveChanges();
            }

            if (!context.Notifications.Any())
            {
                var notifications = new List<Notification>
                {
                    new Notification
                    {
                        UserId = 2,
                        NotificationType = NotificationType.NewTicket,
                        TicketId = 1,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false // ✔️ Eklendi
                    },
                    new Notification
                    {
                        UserId = 3,
                        NotificationType = NotificationType.NewMessage,
                        TicketId = 1,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false // ✔️ Eklendi
                    }
                };
                context.Notifications.AddRange(notifications);
                context.SaveChanges();
            }
        }

        // ✔️ Şifre Hashleme Metodu
        private static string HashPassword(string password)
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
    }
}
