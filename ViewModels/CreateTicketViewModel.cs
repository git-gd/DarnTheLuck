using System.ComponentModel.DataAnnotations;

namespace DarnTheLuck.ViewModels
{
    public class CreateTicketViewModel
    {
        [Required(ErrorMessage="Contact Name is required")]
        public string ContactName { get; set; }
        [Required(ErrorMessage="Contact Phone is required")]
        [Phone]
        public string ContactPhone { get; set; }
        [Required(ErrorMessage="Contact Email Address is required")]
        [EmailAddress]
        public string ContactEmail { get; set; }
        public string TicketNotes { get; set; }
        public string Model { get; set; }
        public string Serial { get; set; }
    }
}