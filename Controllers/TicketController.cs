﻿using DarnTheLuck.Data;
using DarnTheLuck.Helpers;
using DarnTheLuck.Models;
using DarnTheLuck.ViewModels;
using Microsoft.AspNetCore.Authorization;
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

        public async Task<IActionResult> Index(string sort, string sortDir, string search, List<string> sbox, int page = 1, int pageSize = 3)
        {
            ViewBag.sort = string.IsNullOrEmpty(sort) ? "ticket" : sort;
            ViewBag.sortDir = sortDir;
            ViewBag.pageSize = pageSize;
            ViewBag.search = search;
            ViewBag.sbox = sbox;

            IdentityUser user = await _userManager.GetUserAsync(HttpContext.User);

            IList<string> currentUserRoles = await _userManager.GetRolesAsync(user);

            bool isElevated = currentUserRoles.Intersect(elevated).Any();

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

            /**************************************************
             * NOTE: The Where Query BELOW is case INSENSITIVE
             * This is a LIKELY source of future bugs
             * 
             * ALSO: created is a DATE which causes odd behavior
             * you can search on numeric values but you cannot
             * include format characters
             **************************************************/
            // set search value
            if (search != null && sbox.Count > 0)
            {
                ticketListQuery = ticketListQuery.Where(q =>
                    (sbox.Contains("ticket") && q.TicketId.ToString().Contains(search)) ||
                    (sbox.Contains("created") && q.Created.Date.ToString().Contains(search)) || // Date so we don't get time values
                    (sbox.Contains("status") && q.Status.Contains(search)) ||
                    (sbox.Contains("model") && q.Model.Contains(search)) ||
                    (sbox.Contains("serial") && q.Serial.Contains(search)) 
                );
            }

            // set sort method
            ticketListQuery = sortDir == "descending"
                ? (sort switch
                {
                    "status" => ticketListQuery.OrderByDescending(t => t.Status),
                    "created" => ticketListQuery.OrderByDescending(t => t.Created),
                    "model" => ticketListQuery.OrderByDescending(t => t.Model),
                    "serial" => ticketListQuery.OrderByDescending(t => t.Serial),
                    _ => ticketListQuery.OrderByDescending(t => t.TicketId),
                })
                : (sort switch
                {
                    "status" => ticketListQuery.OrderBy(t => t.Status),
                    "created" => ticketListQuery.OrderBy(t => t.Created),
                    "model" => ticketListQuery.OrderBy(t => t.Model),
                    "serial" => ticketListQuery.OrderBy(t => t.Serial),
                    _ => ticketListQuery.OrderBy(t => t.TicketId),
                });

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

        [HttpGet("/ticket/details/{id?}")]
        public async Task<IActionResult> Details(int Id)
        {
            IdentityUser user = await _userManager.GetUserAsync(HttpContext.User);

            IList<string> currentUserRoles = await _userManager.GetRolesAsync(user);

            bool isAdmin = currentUserRoles.Contains("Admin");
            bool isTech  = currentUserRoles.Contains("Technician");

            bool isElevated = isAdmin || isTech;

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
                ticketView = new TicketViewModel(ticket)
                {
                    IsAdmin = isAdmin,
                    IsTech = isTech,
                    IsOwner = ticket.UserId == user.Id
                };
            }

            if (isTech) // Intentionally leaving Admins out here, only Technicians can change Ticket Status (for demonstration)
            {
                ViewBag.ticketStatusList = await _context.TicketStatuses.ToListAsync();
            }

            return View(ticketView);
        }


        /***********************
         * UPDATE Ticket Fields 
         ***********************
         *
         * The way this should be done is with a separate Technician controller and appropriate ViewModels
         * 
         * Clumping this all together in a single View/Controller allows for "interesting" albeit janky solutions
         * 
         */

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(string setField, string setValue, int Id)
        {
            IdentityUser user = await _userManager.GetUserAsync(HttpContext.User);
            List<int> validStatus = await _context.TicketStatuses.Select(ts => ts.Id).ToListAsync();
            
            bool isTech = await _userManager.IsInRoleAsync(user, "Technician");

            Ticket ticket = await _context.Tickets.FindAsync(Id);

            if (ticket != null) // Null check
            {
                if (isTech)
                {
                    switch (setField)
                    {
                        case ("Tech"):
                            ticket.TechName = user.UserName;
                            ticket.TechEmail = user.Email;
                            break;
                        case ("Status"):
                            if (System.Int32.TryParse(setValue, out int value) && validStatus.Contains(value))
                            {
                                ticket.TicketStatusId = value;
                            }
                            break;
                    }
                }
                if (user.Id == ticket.UserId) // User is the Ticket Owner
                {
                    switch (setField)
                    {
                        default:
                            break;
                    }
                }

                _context.SaveChanges();
            }
            return Redirect("/ticket/details/" + Id);
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTicket(int confirm, int Id)
        {
            // verify we have confirmation
            if (confirm == Id)
            {
                Ticket ticket = await _context.Tickets.FindAsync(Id);
            
                if(ticket != null)
                {
                    _context.Tickets.Remove(ticket);
                    await _context.SaveChangesAsync();
                }
            }

            return Redirect("/ticket/details/" + Id);
        }
    }       //TODO: Info Pages - (i)Show code snippets, how the page works, what the features are
}