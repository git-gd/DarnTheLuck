using DarnTheLuck.ViewModels;
using Microsoft.AspNetCore.Identity;
using System;

namespace DarnTheLuck.Models
{
    public class Ticket
    {
        /*
         * Some fields may seem to be unnecessarily duplicated.
         * It is important that a Ticket preserves historical data.
         * We do not want information updates to change past records.
         */

        public int TicketId { get; set; }
        public DateTime Created { get; private set; }

        /*
         * Store the UserId of the user that created the ticket.
         */
        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        /*
         * Name, Phone and Email can be pulled from UserIdentity as default values
         */
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }

        public string TicketNotes { get; set; }

        /*
         * Ticket Status - currently just has an Id and Name
         */
        public int TicketStatusId { get; set; }
        public TicketStatus TicketStatus { get; set; }

        /*
         * Device - could be expanded on and placed in its own table, but this isn't beneficial, yet
         */
        public string Model { get; set; }
        public string Serial { get; set; }

        /*
         * The TechId field will be set by the Technician when the Technician claims the ticket
         * TechId can be blank/null and can change after it is set (if a new Technician takes over)
         * We will use this property in a Join to supply the Tech Name and Email
         */
        public int TechId { get; set; }

        /*
         * Issues (DropDown) - Not implemented
         * 
         * This is a many to many relationsihp. Each ticket can have many issues and each issue can
         * belong to many tickets.
         * 
         * We do not store Issue Ids here. We use a Join Table to set this relationship up.
         */

        public Ticket()
        {
            Created = DateTime.UtcNow; // UTC - keep time zones in mind
        }

        public Ticket(CreateTicketViewModel ticket, string userId) : this()
        {
            UserId = userId;
            ContactName = ticket.ContactName;
            ContactEmail = ticket.ContactEmail;
            ContactPhone = ticket.ContactPhone;
            TicketNotes = ticket.TicketNotes;
            TicketStatusId = 1;
            Model = ticket.Model;
            Serial = ticket.Serial;
        }
    }
}
