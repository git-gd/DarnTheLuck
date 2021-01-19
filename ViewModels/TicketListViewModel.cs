using DarnTheLuck.Models;
using System;

namespace DarnTheLuck.ViewModels
{
    public class TicketListViewModel
    {
        public int TicketId { get; set; }
        public string Status { get; set; }
        public DateTime Created { get; set; }
        public string Model { get; set; }
        public string Serial { get; set; }

        public TicketListViewModel(Ticket ticket)
        {
            TicketId = ticket.TicketId;
            Created = ticket.Created;
            Status = ticket.TicketStatus.Name;
            Model = ticket.Model;
            Serial = ticket.Serial;
        }
    }
}
