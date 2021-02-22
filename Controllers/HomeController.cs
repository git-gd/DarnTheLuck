using DarnTheLuck.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;

namespace DarnTheLuck.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            // Free Chuck Norris Joke API
            DarnTheLuck.Helpers.Chuck chuck = new DarnTheLuck.Helpers.Chuck();

            ViewBag.Joke = chuck.Joke;

            ViewBag.Trouble = new string[] { Environment.GetEnvironmentVariable("WEBSITE_MYSQL_PORT") };

            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}