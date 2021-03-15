using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

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
        public static async Task CreateNoteAsync(DarnTheLuck.Data.ApplicationDbContext source, string userId, int ticketId, string note)
        {
            await source.TicketNotes.AddAsync(new TicketNotes() {
                TicketId = ticketId,
                UserId   = userId,
                Note     = note
            });
            await source.SaveChangesAsync();
        }
    }
}
