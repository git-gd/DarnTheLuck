﻿using DarnTheLuck.Models;
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
        public string Status { get; set; }
        public string Model { get; set; }
        public string Serial { get; set; }
        public string TechName { get; set; }
        public string TechEmail { get; set; }

        public TicketViewModel(Ticket ticket)
        {
            TicketId = ticket.TicketId;
            Created = ticket.Created;
            ContactName = ticket.ContactName;
            ContactPhone = ticket.ContactPhone;
            ContactEmail = ticket.ContactEmail;
            TicketNotes = ticket.TicketNotes;
            Status = ticket.TicketStatus.Name;
            Model = ticket.Model;
            Serial = ticket.Serial;

            //TODO: Add Tech Info - need to setup relationship and pull in extra info
            TechName = "TODO: TechName";
            TechEmail = "TODO: TechEmail";
        }
    }
}