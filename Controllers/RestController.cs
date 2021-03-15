using DarnTheLuck.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DarnTheLuck.Controllers
{
    [Route("api/commands")]
    [ApiController]
    public class RestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public RestController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public ActionResult GetAllCommands()
        {
            //TODO: Very beginning of API
            return Ok();
        }
    }
}
//https://www.youtube.com/watch?v=fmvcAzHpsk8
