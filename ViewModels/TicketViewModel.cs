using DarnTheLuck.Models;
using System.Collections.Generic;

namespace DarnTheLuck.ViewModels
{
    public class TicketViewModel
    {
        public bool IsOwner { get; set; } = false;

        public int TicketId { get; set; }
        public string Created { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public string TicketNotes { get; set; }
        public ICollection<TicketNotes> TicketNoteList { get; set; }
        public string Status { get; set; }
        public string Model { get; set; }
        public string Serial { get; set; }
        public string TechName { get; set; }
        public string TechEmail { get; set; }

        public TicketViewModel(Ticket ticket)
        {
            TicketId = ticket.TicketId;
            Created = ticket.Created.ToString("yyyy-MM-dd");
            ContactName = ticket.ContactName;
            ContactPhone = ticket.ContactPhone;
            ContactEmail = ticket.ContactEmail;
            TicketNotes = ticket.TicketNotes;
            TicketNoteList = ticket.TicketNoteList;
            Status = ticket.TicketStatus.Name;
            Model = ticket.Model;
            Serial = ticket.Serial;
            TechName = ticket.TechName;
            TechEmail = ticket.TechEmail;
        }
    }
}
