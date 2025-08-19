using System.ComponentModel.DataAnnotations;

namespace Ticket_Automation.Entity
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        public NotificationType NotificationType { get; set; }

        [Required]
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsRead { get; set; } = false; // ✔️ Okundu bilgisi eklendi
    }
}
