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

        public string CreatedString { get { return Created.ToString("yyyy-MM-dd"); } }
        public TicketListViewModel() { }
    }
}