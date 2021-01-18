using DarnTheLuck.Data;
using DarnTheLuck.Models;
using DarnTheLuck.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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

                TicketViewModel ticketView = new TicketViewModel(newTicket);

                return View("Details", ticketView);
            }

            return View("Create", ticketModel);
        }

        [HttpGet("/ticket/details/{id?}")]
        public IActionResult Details(int Id)
        {
            Ticket ticket = _context.Tickets
                .FirstOrDefault(t =>
                    t.UserId == _userManager.GetUserId(HttpContext.User) &&
                    t.TicketId == Id);

            TicketViewModel ticketView = (ticket == null)?null:new TicketViewModel(ticket);

            return View(ticketView);
        }
    }
}
