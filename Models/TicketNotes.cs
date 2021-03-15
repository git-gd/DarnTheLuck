using Microsoft.AspNetCore.Identity;

namespace DarnTheLuck.Models
{
    public class TicketNotes
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }
        public string Note { get; set; }
    }
}
