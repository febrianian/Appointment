using Appointment.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Appointment.Controllers
{
    //[Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppointmentContext _context;

        public HomeController(ILogger<HomeController> logger, AppointmentContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var spesialis = _context.Spesialis.Where(i => i.Status == "A").ToList();
            ViewData["DataSpesialis"] = spesialis;
            return View(spesialis);
        }

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