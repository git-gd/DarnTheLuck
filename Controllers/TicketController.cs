using DarnTheLuck.Data;
using DarnTheLuck.Models;
using DarnTheLuck.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DarnTheLuck.Controllers
{
    public class TicketController : Controller
    {
        /*****************************************************
        * NORMAL USERS CAN ONLY VIEW TICKETS THEY HAVE CREATED
        ******************************************************
        * 
        * When a Ticket is created, the User's Id is stored in the UserId field of the new ticket.
        * 
        * When a user attempts to view a list of tickets we filter our database results by the current User's UserId.
        * 
        *************************************
        * ELEVATED USERS CAN VIEW ALL TICKETS
        *************************************
        * 
        * We include all Tickets in our database results if the current user is an Admin or Tech
        */


        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        private string[] elevated = new string[] { "Admin", "Technician" }; // temporary spot to store our elevated roles

        public TicketController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /*
         *  Learning opportunity: ASYNC - Task<>, await
         */

        public async Task<IActionResult> Index()
        {
            List<TicketListViewModel> ticketList = new List<TicketListViewModel>();

            IdentityUser user = await _userManager.GetUserAsync(HttpContext.User);

            IList<string> currentUserRoles = await _userManager.GetRolesAsync(user);

            bool isElevated = currentUserRoles.Select(x => x)
                          .Intersect(elevated)
                          .Any();

            List<Ticket> tickets = _context.Tickets
                .Include(t => t.TicketStatus) // so we can access the Name string in the related table
                .Where(t =>
                    t.UserId == user.Id || // match UserId - individuals can access their ticket
                    isElevated)            // match RoleId - Techs can access all tickets
                .ToList();

            foreach (Ticket ticket in tickets)
            {
                TicketListViewModel ticketListItem = new TicketListViewModel(ticket);
                ticketList.Add(ticketListItem);
            }

            return View(ticketList);
        }

        [HttpGet]
        public IActionResult Create()
        {
            CreateTicketViewModel ticket = new CreateTicketViewModel()
            {
                ContactEmail = User.Identity.Name,
            };

            return View(ticket);
        }

        [HttpPost]
        [ActionName("Create")]
        public IActionResult SaveTicket(CreateTicketViewModel ticketModel)
        {
            if (ModelState.IsValid)
            {
                /*
                 * Every ticket MUST have a status
                 * If there are no valid ticket statuses, create one
                 */

                TicketStatus ticketStatus = _context.TicketStatuses.FirstOrDefault();

                if (ticketStatus == null)
                {
                    ticketStatus = new TicketStatus()
                    {
                        /*
                         * MySQL can set the Id but I chose to hard-code these values on initialization
                         */
                        Id = 1,
                        Name = "Created"
                    };
                    _context.TicketStatuses.Add(ticketStatus);
                    _context.SaveChanges();
                }

                string userId = _userManager.GetUserId(HttpContext.User);

                Ticket newTicket = new Ticket(ticketModel, userId);

                _context.Tickets.Add(newTicket);
                _context.SaveChanges();

                TicketViewModel ticketView = new TicketViewModel(newTicket);

                return View("Details", ticketView);
            }

            return View("Create", ticketModel);
        }

        [HttpGet("/ticket/details/{id?}")] // This enables routing that we can use in a link
        public async Task<IActionResult> Details(int Id)
        {
            // ****** DRY?
            IdentityUser user = await _userManager.GetUserAsync(HttpContext.User);

            IList<string> currentUserRoles = await _userManager.GetRolesAsync(user);

            bool isElevated = currentUserRoles.Select(x => x)
                          .Intersect(elevated)
                          .Any();
            // ****** DRY?

            Ticket ticket = _context.Tickets
                .Include(t => t.TicketStatus) // so we can access the Name string in the related table
                .FirstOrDefault(t =>
                    t.UserId == user.Id ||    // match UserId - individuals can access their ticket details
                    isElevated &&             // allow Elevated users (Admin, Tech) to view details
                    t.TicketId == Id);

            TicketViewModel ticketView = (ticket == null) ? null : new TicketViewModel(ticket);

            return View(ticketView);
        }
    }
}