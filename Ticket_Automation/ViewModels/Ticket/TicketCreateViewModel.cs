using System.ComponentModel.DataAnnotations;
using Ticket_Automation.Entity;

namespace Ticket_Automation.ViewModels.Ticket
{
    public class TicketCreateViewModel
    {
        [Required(ErrorMessage = "Başlık zorunludur.")]
        [MaxLength(100, ErrorMessage = "Başlık en fazla 100 karakter olabilir.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Açıklama zorunludur.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Öncelik seçilmelidir.")]
        public TicketPriority Priority { get; set; }
        public IFormFile? Attachment { get; set; }
    }
}
