using DarnTheLuck.Data;
using DarnTheLuck.Models;
using DarnTheLuck.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DarnTheLuck.Controllers
{
    [Authorize]
    public class TicketController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TicketController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
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
                Ticket newTicket = new Ticket()
                {
                    UserId = _userManager.GetUserId(HttpContext.User),
                    ContactName = ticketModel.ContactName,
                    ContactEmail = ticketModel.ContactEmail,
                    ContactPhone = ticketModel.ContactPhone,
                    TicketNotes = ticketModel.TicketNotes
                };

                _context.Tickets.Add(newTicket);
                _context.SaveChanges();

                TicketViewModel ticketView = new TicketViewModel()
                {
                    TicketId = newTicket.TicketId,
                    ContactName = newTicket.ContactName,
                    TicketNotes = newTicket.TicketNotes,
                    Created = newTicket.Created
                };

                // redirect to a view of the ticket
                return View("Details", ticketView);
            }

            return View("Create", ticketModel);
        }        

        public IActionResult Details()
        {


            return View();
        }
    }
}
