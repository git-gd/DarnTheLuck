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

        //TODO: Complete Ticket Model
        /*
         * Ticket Status
         * Device
         * Issues (DropDown)
         * Technician
         */

        public Ticket()
        {
            Created = DateTime.UtcNow;
        }
    }
}
