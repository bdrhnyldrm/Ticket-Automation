using System.ComponentModel.DataAnnotations;

namespace Ticket_Automation.Entity
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;
        public string? AttachmentPath { get; set; }

        [Required]
        public int CreatedById { get; set; }
        public User CreatedBy { get; set; } = null!;

        public int? AssignedToId { get; set; }
        public User? AssignedTo { get; set; }

        public TicketPriority? Priority { get; set; }

        [Required]
        public TicketStatus Status { get; set; } = TicketStatus.Open;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ClosedAt { get; set; }

        // Navigation Properties
        public ICollection<TicketMessage> Messages { get; set; } = new List<TicketMessage>();
    }
}
