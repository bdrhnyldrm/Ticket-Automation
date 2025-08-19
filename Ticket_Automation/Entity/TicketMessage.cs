using System.ComponentModel.DataAnnotations;

namespace Ticket_Automation.Entity
{
    public class TicketMessage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        [Required]
        public int SenderId { get; set; }
        public User Sender { get; set; } = null!;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
