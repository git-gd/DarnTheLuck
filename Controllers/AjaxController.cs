﻿using DarnTheLuck.Data;
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
    public class AjaxController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        private readonly string[] elevated = new string[] { "Admin", "Technician" }; // temporary spot to store our elevated roles

        public AjaxController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
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
                where ((Ticket.UserId == user.Id ||
                        isElevated ||
                        grantIds.Contains(Ticket.UserId)) &&
                        (string.IsNullOrEmpty(tIViewModel.Search) ||
                            (tIViewModel.Sbox.Contains("ticket") && Ticket.TicketId.ToString().Contains(tIViewModel.Search)) ||
                            (tIViewModel.Sbox.Contains("created") && Ticket.Created.Date.ToString().Contains(tIViewModel.Search)) ||
                            (tIViewModel.Sbox.Contains("status") && Ticket.TicketStatus.Name.Contains(tIViewModel.Search)) ||
                            (tIViewModel.Sbox.Contains("model") && Ticket.Model.Contains(tIViewModel.Search)) ||
                            (tIViewModel.Sbox.Contains("serial") && Ticket.Serial.Contains(tIViewModel.Search))
                        )
                )
                select new TicketListViewModel()
                {
                    TicketId = Ticket.TicketId,
                    Created = Ticket.Created,
                    Status = Ticket.TicketStatus.Name,
                    Model = Ticket.Model,
                    Serial = Ticket.Serial
                });

            //set sort method
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

        [HttpGet("/ajax/details/{id?}")]
        public async Task<IActionResult> Details(int Id)
        {
            IdentityUser user = await _userManager.GetUserAsync(HttpContext.User);

            IList<string> currentUserRoles = await _userManager.GetRolesAsync(user);

            bool isElevated = currentUserRoles.Intersect(elevated).Any();

            List<string> grantIds = await _context.UserGroups
                .Where(u => u.GrantId == user.Id && u.Authorized)
                .Select(u => u.UserId)
                .ToListAsync();

            Ticket ticket = await _context.Tickets
                .Include(t => t.TicketStatus)       // so we can access the Name string in the related table
                .FirstOrDefaultAsync(t =>
                    (t.UserId == user.Id ||       // match UserId - individuals can access their ticket details
                       isElevated ||                // allow Elevated users (Admin, Tech) to view details
                       grantIds.Contains(t.UserId)) // allow users who have been granted access to view details
                    && t.TicketId == Id);

            TicketViewModel ticketView = ticket is null
                ? null
                : new TicketViewModel(ticket)
                {
                    IsOwner = ticket.UserId == user.Id
                };

            if (User.IsInRole("Technician")) // Intentionally leaving Admins out here, only Technicians can change Ticket Status
            {
                ViewBag.ticketStatusList = await _context.TicketStatuses.ToListAsync();
            }

            return PartialView(ticketView);
        }
    }
}
