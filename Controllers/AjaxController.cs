using DarnTheLuck.Data;
using DarnTheLuck.Helpers;
using DarnTheLuck.Models;
using DarnTheLuck.ViewModels;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
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

        public async Task<IActionResult> TestEmail()
        {
            MailjetClient client = new MailjetClient(Ring.MailjetKey1, Ring.MailjetKey2);
            MailjetRequest request = new MailjetRequest
            {
                Resource = Send.Resource,
            }
               .Property(Send.FromEmail, "FROMEMAILADDRESS")
               .Property(Send.FromName, "DTL")
               .Property(Send.Subject, "Test Email")
               .Property(Send.TextPart, "Dear passenger, welcome to Mailjet! May the delivery force be with you!")
               .Property(Send.HtmlPart, "<h3>Dear passenger, welcome to <a href=\"https://www.mailjet.com/\">Mailjet</a>!<br />May the delivery force be with you!")
               .Property(Send.Recipients, new JArray {
                new JObject {
                 {"Email", "TOEMAILADDRESS"}
                 }
                   });

            MailjetResponse response = await client.PostAsync(request);
            if (response.IsSuccessStatusCode)
            {
                string s1 = string.Format("Total: {0}, Count: {1}\n", response.GetTotal(), response.GetCount());
                string s2 = response.GetData().ToString();
            }
            else
            {
                string s3 = string.Format("StatusCode: {0}\n", response.StatusCode);
                string s4 = string.Format("ErrorInfo: {0}\n", response.GetErrorInfo());
                string s5 = response.GetData().ToString();
                string s6 = string.Format("ErrorMessage: {0}\n", response.GetErrorMessage());
            }

            return Redirect("Index");
        }
    }
}
