using DarnTheLuck.ViewModels;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace DarnTheLuck.Models
{
    public class Ticket
    {
        public int TicketId { get; set; }
        public DateTime Created { get; private set; }

        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }

        public string TicketNotes { get; set; }
        public ICollection<TicketNotes> TicketNoteList { get; set; }

        public int TicketStatusId { get; set; }
        public TicketStatus TicketStatus { get; set; }

        public string Model { get; set; }
        public string Serial { get; set; }

        public string TechName { get; set; }
        public string TechEmail { get; set; }


        public Ticket()
        {
            Created = DateTime.UtcNow; // UTC - Consider using local time
        }

        public Ticket(CreateTicketViewModel ticket, string userId, int status) : this()
        {
            UserId = userId;
            ContactName = ticket.ContactName;
            ContactEmail = ticket.ContactEmail;
            ContactPhone = ticket.ContactPhone;
            TicketNotes = ticket.TicketNotes;
            TicketStatusId = status;
            Model = ticket.Model;
            Serial = ticket.Serial;
        }
    }
}
