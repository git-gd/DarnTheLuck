﻿using DarnTheLuck.Data;
using DarnTheLuck.Helpers;
using DarnTheLuck.Models;
using DarnTheLuck.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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
        * 
        * *****************
        * PERMISSION GRANTS
        * *****************
        * Users can create a code that can be shared with other users. Once a code has been created, shared and consumed the
        * user that consumes the code can view (but not change) the creator's tickets.
        * 
        */


        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        private readonly string[] elevated = new string[] { "Admin", "Technician" }; // temporary spot to store our elevated roles

        public TicketController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
      
        public async Task<IActionResult> Index(TicketIndexViewModel tIViewModel)
        {
            IdentityUser user = await _userManager.GetUserAsync(HttpContext.User);
            
            IList<string> currentUserRoles = await _userManager.GetRolesAsync(user);
          
            bool isElevated = currentUserRoles.Intersect(elevated).Any();

            List<string> grantIds = await _context.UserGroups
                .Where(u => u.GrantId == user.Id && u.Authorized)
                .Select(u => u.UserId)
                .ToListAsync();

            IQueryable<TicketListViewModel> ticketListQuery = (
                from Ticket in _context.Tickets
                where (Ticket.UserId == user.Id || isElevated || grantIds.Contains(Ticket.UserId))
                select new TicketListViewModel()
                {
                    TicketId = Ticket.TicketId,
                    Created = Ticket.Created.ToString(), //.ToShortDateString(),
                    Status = Ticket.TicketStatus.Name,
                    Model = Ticket.Model,
                    Serial = Ticket.Serial
                });

            /**************************************************
             * NOTE: The Where Query BELOW is case INSENSITIVE
             * This is a LIKELY source of future bugs
             **************************************************/
            // set search value
            if (tIViewModel.Search != null && tIViewModel.Sbox.Count > 0)
            {
                ticketListQuery = ticketListQuery.Where(q =>
                    (tIViewModel.Sbox.Contains("ticket") && q.TicketId.ToString().Contains(tIViewModel.Search)) ||
                    (tIViewModel.Sbox.Contains("created") && q.Created.Contains(tIViewModel.Search)) ||
                    (tIViewModel.Sbox.Contains("status") && q.Status.Contains(tIViewModel.Search)) ||
                    (tIViewModel.Sbox.Contains("model") && q.Model.Contains(tIViewModel.Search)) ||
                    (tIViewModel.Sbox.Contains("serial") && q.Serial.Contains(tIViewModel.Search))
                );
            }

            // set sort method
            ticketListQuery = tIViewModel.SortDir == "descending"
                ? (tIViewModel.Sort switch
                {
                    "status" => ticketListQuery.OrderByDescending(t => t.Status),
                    "created" => ticketListQuery.OrderByDescending(t => t.Created),
                    "model" => ticketListQuery.OrderByDescending(t => t.Model),
                    "serial" => ticketListQuery.OrderByDescending(t => t.Serial),
                    _ => ticketListQuery.OrderByDescending(t => t.TicketId),
                })
                : (tIViewModel.Sort switch
                {
                    "status" => ticketListQuery.OrderBy(t => t.Status),
                    "created" => ticketListQuery.OrderBy(t => t.Created),
                    "model" => ticketListQuery.OrderBy(t => t.Model),
                    "serial" => ticketListQuery.OrderBy(t => t.Serial),
                    _ => ticketListQuery.OrderBy(t => t.TicketId),
                });

            tIViewModel.TicketList = await PaginatedList<TicketListViewModel>.CreateAsync(ticketListQuery, tIViewModel.Page, tIViewModel.PageSize);

            return View(tIViewModel);
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
                 * If there are no valid ticket statuses, create them
                 */

                TicketStatus ticketStatus = _context.TicketStatuses.FirstOrDefault(ts => ts.Name == "Created");

                if (ticketStatus == null)
                {
                    string[] statuses =
                    {
                        "Created",
                        "Limbo",
                        "Ready",
                        "Shipped"
                    };

                    foreach (string status in statuses)
                    {
                        ticketStatus = new TicketStatus()
                        {
                            Name = status
                        };
                        _context.TicketStatuses.Add(ticketStatus);
                    }

                    _context.SaveChanges(); // if > 0 success...
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

            List<string> grantIds = await _context.UserGroups
                .Where(u => u.GrantId == user.Id && u.Authorized)
                .Select(u => u.UserId)
                .ToListAsync();

            Ticket ticket = await _context.Tickets
                .Include(t => t.TicketStatus)  // so we can access the Name string in the related table
                .FirstOrDefaultAsync(t =>
                    (  t.UserId == user.Id ||  // match UserId - individuals can access their ticket details
                       isElevated ||           // allow Elevated users (Admin, Tech) to view details
                       grantIds.Contains(t.UserId)) // allow users who have been granted access to view details
                    && t.TicketId == Id);

            TicketViewModel ticketView = ticket is null
                ? null
                : new TicketViewModel(ticket)
                {
                    IsAdmin = isAdmin,
                    IsTech = isTech,
                    IsOwner = ticket.UserId == user.Id
                };


            if (isTech) // Intentionally leaving Admins out here, only Technicians can change Ticket Status (for demonstration)
            {
                ViewBag.ticketStatusList = await _context.TicketStatuses.ToListAsync();
            }

            return View(ticketView);
        }

        /*
         * Technicians can update the Ticket Tech fields
         */

        [Authorize(Roles = "Technician")]
        [HttpPost]
        public async Task<IActionResult> UpdateTech(int Id)
        {
            IdentityUser user = await _userManager.GetUserAsync(HttpContext.User);

            Ticket ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.TicketId == Id);

            if (ticket != null)
            {               
                ticket.TechName = user.UserName;
                ticket.TechEmail = user.Email;

                await _context.SaveChangesAsync();
            }
            return Redirect("/ticket/details/" + Id);
        }

        /*
         * Technicians can change the Ticket Status
         */

        [Authorize(Roles ="Technician")]
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int status, int Id)
        {
            bool validStatus = await _context.TicketStatuses.Select(ts => ts.Id).ContainsAsync(status);

            Ticket ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.TicketId == Id);


            if (ticket != null && validStatus)
            {
                ticket.TicketStatusId = status;

                await _context.SaveChangesAsync();
            }
            return Redirect("/ticket/details/" + Id);
        }

        /*
         * Currently only allows Ticket Owners to update Ticket Notes if the Ticket Status is not Shipped
         */
        [HttpPost]
        public async Task<IActionResult> UpdateTicket(string notes, int Id)
        {
            IdentityUser user = await _userManager.GetUserAsync(HttpContext.User);
            Ticket ticket = await _context.Tickets
                .Include(t => t.TicketStatus)
                .FirstOrDefaultAsync(t => t.TicketId == Id);

            if (ticket != null)
            {
                if (user.Id == ticket.UserId && ticket.TicketStatus.Name != "Shipped")
                {
                    ticket.TicketNotes = notes;
                    await _context.SaveChangesAsync();
                }
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