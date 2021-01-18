using DarnTheLuck.Models;
using System;

namespace DarnTheLuck.ViewModels
{
    public class TicketViewModel
    {
        public int TicketId { get; set; }
        public DateTime Created { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public string TicketNotes { get; set; }

        public TicketViewModel(Ticket ticket)
        {
            TicketId = ticket.TicketId;
            ContactName = ticket.ContactName;
            TicketNotes = ticket.TicketNotes;
            Created = ticket.Created;
        }
    }
}
