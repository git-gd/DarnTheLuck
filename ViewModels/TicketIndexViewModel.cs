using DarnTheLuck.Data;
using DarnTheLuck.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DarnTheLuck.ViewModels
{
    public class TicketIndexViewModel
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 3;
        public string Sort { get; set; } = "Ticket";
        public string SortDir { get; set; }
        public string Search { get; set; }
        public List<string> Sbox { get; set; } = new List<string> { "ticket", "status", "model", "serial" };

        public PaginatedList<TicketListViewModel> TicketList { get; set; }

        public TicketIndexViewModel()
        {
        }
    }
}
