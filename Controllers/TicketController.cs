using DarnTheLuck.Data;
using DarnTheLuck.Helpers;
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

        private readonly string[] elevated = new string[] { "Admin", "Technician" }; // temporary spot to store our elevated roles

        public TicketController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /*
         *  Learning opportunity: ASYNC - Task<>, await
         */

        public async Task<IActionResult> Index(string sort, string sortDir, int page = 1)
        {
            const int pageSize = 3; // How many records do we want to list?

            ViewBag.sort = string.IsNullOrEmpty(sort)?"ticket":sort;
            ViewBag.sortDir = sortDir;
            //ViewBag.sortDir = string.IsNullOrEmpty(sortDir) ? string.Empty : "descending";

            IdentityUser user = await _userManager.GetUserAsync(HttpContext.User);

            IList<string> currentUserRoles = await _userManager.GetRolesAsync(user);

            bool isElevated = currentUserRoles.Intersect(elevated).Any();

            //TODO: Search (collapsable form, text input, checkbox properties/fields)

            IQueryable<TicketListViewModel> ticketListQuery = (
                from Ticket in _context.Tickets
                where (Ticket.UserId == user.Id || isElevated)
                select new TicketListViewModel()
                {
                    TicketId = Ticket.TicketId,
                    Created = Ticket.Created,
                    Status = Ticket.TicketStatus.Name,
                    Model = Ticket.Model,
                    Serial = Ticket.Serial
                });

            /*
             * TODO: This is so ugly... there has to be a better way
             */

            // set sort method
            if (sortDir == "descending")
            {
                switch (sort)
                {
                    case "status":
                        ticketListQuery = ticketListQuery.OrderByDescending(t => t.Status);
                        break;
                    case "created":
                        ticketListQuery = ticketListQuery.OrderByDescending(t => t.Created);
                        break;
                    case "model":
                        ticketListQuery = ticketListQuery.OrderByDescending(t => t.Model);
                        break;
                    case "serial":
                        ticketListQuery = ticketListQuery.OrderByDescending(t => t.Serial);
                        break;
                    default: // TicketId
                        ticketListQuery = ticketListQuery.OrderByDescending(t => t.TicketId);
                        break;
                }
            }
            else
            {
                switch (sort)
                {
                    case "status":
                        ticketListQuery = ticketListQuery.OrderBy(t => t.Status);
                        break;
                    case "created":
                        ticketListQuery = ticketListQuery.OrderBy(t => t.Created);
                        break;
                    case "model":
                        ticketListQuery = ticketListQuery.OrderBy(t => t.Model);
                        break;
                    case "serial":
                        ticketListQuery = ticketListQuery.OrderBy(t => t.Serial);
                        break;
                    default: // TicketId
                        ticketListQuery = ticketListQuery.OrderBy(t => t.TicketId);
                        break;
                }
            }

            PaginatedList<TicketListViewModel> ticketList = await PaginatedList<TicketListViewModel>.CreateAsync(ticketListQuery, page, pageSize);

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

                /*
                 * TODO: Query Past User Tickets To Pull Contact Info Or Table Link?
                 */

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

            bool isAdmin = currentUserRoles.Contains("Admin");
            bool isTech  = currentUserRoles.Contains("Technician");

            bool isElevated = isAdmin || isTech;
            // ****** DRY?

            Ticket ticket = _context.Tickets
                .Include(t => t.TicketStatus)  // so we can access the Name string in the related table
                .FirstOrDefault(t =>
                    (  t.UserId == user.Id ||  // match UserId - individuals can access their ticket details
                       isElevated)             // allow Elevated users (Admin, Tech) to view details
                    && t.TicketId == Id);

            TicketViewModel ticketView;

            /*
             * Below is one way to test Roles, another is to inject AuthorizationService into the page (Home/Index does this).
             * 
             * Another possibile way to do this would be create an Elevated controller and do our Role checks there
            */

            if (ticket is null){
                ticketView = null;
            } else {
                ticketView = new TicketViewModel(ticket);
                ticketView.IsAdmin = isAdmin;
                ticketView.IsTech  = isTech;   
                ticketView.IsOwner = ticket.UserId == user.Id;
            }

            return View(ticketView);
        }
    }       //TODO: Info Pages - (i)Show code snippets, how the page works, what the features are
}