using DarnTheLuck.Models;
using System.Collections.Generic;

namespace DarnTheLuck.ViewModels
{
    public class EditStatusViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Delete { get; set; }
        public List<TicketStatus> TicketStatuses { get; set; }
    }
}
