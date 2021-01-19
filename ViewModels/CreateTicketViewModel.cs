using System.ComponentModel.DataAnnotations;

namespace DarnTheLuck.ViewModels
{
    public class CreateTicketViewModel
    {
        [Required]
        public string ContactName { get; set; }
        [Required]
        public string ContactPhone { get; set; }
        [Required]
        public string ContactEmail { get; set; }
        public string TicketNotes { get; set; }
        public string Model { get; set; }
        public string Serial { get; set; }
    }
}
