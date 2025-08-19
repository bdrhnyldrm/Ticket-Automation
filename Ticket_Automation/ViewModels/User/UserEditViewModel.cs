using System.ComponentModel.DataAnnotations;
using Ticket_Automation.Entity;

namespace Ticket_Automation.ViewModels.User
{
    public class UserEditViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone, MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        public UserRole Role { get; set; }
    }
}
