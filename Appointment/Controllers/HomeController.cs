using Appointment.Models;
using Appointment.Services;
using Appointment.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Diagnostics;

namespace Appointment.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppointmentContext _context;
        private readonly IConfiguration _config;

        public HomeController(ILogger<HomeController> logger,IConfiguration config, AppointmentContext context)
        {
            _logger = logger;
            _context = context;
            _config = config;
        }

        [Authorize(Roles = "Doctor, Patient, Admin")]
        public IActionResult Index()
        {
            var spesialis = _context.Spesialis.Where(i => i.Status == "A").ToList();
            ViewData["DataSpesialis"] = spesialis;
            return View(spesialis);
        }

        public async Task<IActionResult> DoctorSpesialis(int idSpesialis)
        {
            var spesialis = (from spes in _context.Spesialis
                            join sche in _context.SpesialisSchedule on spes.Id equals sche.IdSpesialis
                            join usr in _context.Users on sche.UserId equals usr.Id
                            where spes.Id == idSpesialis && spes.Status == "A"
                            select new
                            {
                                spes.Id,
                                spes.SpesialisName,
                                UserId = usr.Id,
                                usr.Name
                            }).Distinct();

            var time = (from sche in _context.SpesialisSchedule
                        join usr in spesialis on sche.UserId equals usr.UserId
                        where sche.Status == "A"
                        select new
                        {
                            sche.IdSpesialisSchedule,
                            sche.UserId,
                            sche.ScheduleDay,
                            sche.StartDate,
                            sche.EndDate
                        }).Distinct();

            List<SpesialisScheduleViewModel> items = new List<SpesialisScheduleViewModel>();
            List<SpesialisScheduleViewModel> itemsTime = new List<SpesialisScheduleViewModel>();
            
            foreach(var data in spesialis.ToList())
            {
                SpesialisScheduleViewModel vm = new SpesialisScheduleViewModel();
                vm.IdSpesialis = data.Id;
                vm.SpesialisName = data.SpesialisName;
                vm.UserId = data.UserId;
                vm.Name = data.Name;
                items.Add(vm);
            }

            foreach (var data in time.ToList().OrderBy(i => i.IdSpesialisSchedule))
            {
                SpesialisScheduleViewModel vm = new SpesialisScheduleViewModel();
                //vm.IdSpesialis = data.Id;
                vm.UserId = data.UserId;
                vm.ScheduleDay = data.ScheduleDay;
                vm.StartDate = data.StartDate.ToString("HH:mm");
                vm.EndDate = data.EndDate.ToString("HH:mm");
                itemsTime.Add(vm);
            }

            var timeGroupedByUserId = itemsTime.GroupBy(t => t.UserId);

            ViewData["IdSpesialis"] = spesialis.FirstOrDefaultAsync().Id;
            ViewData["TimeGroupedByUserId"] = timeGroupedByUserId;

            return View(items);
        }

        [AllowAnonymous]
        public async Task<IActionResult> PrivacyAsync()
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