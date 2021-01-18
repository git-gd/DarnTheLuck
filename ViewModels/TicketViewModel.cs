﻿using System;
using System.ComponentModel.DataAnnotations;

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

    }
}
