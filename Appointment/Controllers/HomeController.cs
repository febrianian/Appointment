using Appointment.Migrations;
using Appointment.Models;
using Appointment.Services;
using Appointment.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
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
            ViewData["DataSpesialis"] = _context.Spesialis.Where(i => i.Status == "A").ToList();
            return View();
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

        public List<SpesialisScheduleViewModel> ListSpesialis(int id)
        {
            List<SpesialisScheduleViewModel> model = new List<SpesialisScheduleViewModel>();
            var result = (from user in _context.Users
                          join schedule in _context.SpesialisSchedule on user.Id equals schedule.UserId
                          join spesialis in _context.Spesialis on schedule.IdSpesialis equals spesialis.Id
                          where spesialis.Status == "A" && spesialis.Id == id
                          select new
                          {
                              spesialis.Id,
                              UserId = user.Id,
                              user.Name,
                              spesialis.SpesialisName,
                              spesialis.Status
                          }).Distinct();

            foreach (var item in result)
            {
                SpesialisScheduleViewModel vm = new SpesialisScheduleViewModel();
                vm.IdSpesialis = item.Id;
                vm.UserId = item.UserId;
                vm.SpesialisName = item.SpesialisName;
                vm.Status = item.Status;
                vm.Name = item.Name;
                model.Add(vm);
            }

            return model;
        }

        public List<SpesialisScheduleViewModel> ListSpesialisHours(int id)
        {
            List<SpesialisScheduleViewModel> model = new List<SpesialisScheduleViewModel>();
            var result = (from time in _context.SpesialisSchedule
                          join usr in _context.Users on time.UserId equals usr.Id
                          where time.IdSpesialis == id && time.Status == "A"
                          select new
                          {
                              time.IdSpesialis,
                              time.IdSpesialisSchedule,
                              usr.Id,
                              usr.Name,
                              time.ScheduleDay,
                              time.StartDate,
                              time.EndDate
                          }).Distinct();


            foreach (var item in result.OrderBy(i => i.IdSpesialisSchedule))
            {
                SpesialisScheduleViewModel vm = new SpesialisScheduleViewModel();
                vm.IdSpesialis = item.IdSpesialis;
                vm.IdSpesialisSchedule = item.IdSpesialisSchedule;
                vm.UserId = item.Id;
                vm.Name = item.Name;
                vm.ScheduleDay = item.ScheduleDay;
                vm.StartDate = item.StartDate.ToString("HH:mm");
                vm.EndDate = item.EndDate.ToString("HH:mm");
                model.Add(vm);
            }
            return model;
        }

        public async Task<IActionResult> DashboardSpesialis(DashboardSpesialisViewModel model)
        {
            model.ListSpesialis = ListSpesialis(model.IdSpesialis);
            //var listSpesialis = ListSpesialis(model.IdSpesialis);
            model.ListSpesialisHours = ListSpesialisHours(model.IdSpesialis);
            //model.ListSpesialisHours = ListSpesialisHours(model.IdSpesialis, listSpesialis.FirstOrDefault().UserId);
            return View(model);
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