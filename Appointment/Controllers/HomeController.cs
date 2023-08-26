using Appointment.Migrations;
using Appointment.Models;
using Appointment.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MimeKit;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        public IActionResult Index(HomeViewModel vm)
        {
            List<AppointmentClinicViewModel> itemsTransactionPatient = new List<AppointmentClinicViewModel>();
            List<AppointmentClinicViewModel> itemsTransactionDoctor = new List<AppointmentClinicViewModel>();
            var transactionPatient = _context.AppointmentClinic.Where(i => i.UserCreated == User.Identity.Name).ToList();

            foreach(var data in transactionPatient)
            {
                AppointmentClinicViewModel item = new AppointmentClinicViewModel();
                item.Spesialis = _context.Spesialis.Where(i => i.Id == data.IdSpesialis).Single().SpesialisName;
                item.PatientName = _context.Users.Where(i => i.Id == data.UserIdPatient).Single().Name;
                item.DoctorName = _context.Users.Where(i => i.Id == data.UserIdDoctor).Single().Name;
                item.Day = data.Day;
                item.TimeAppointment = data.TimeAppointment;
                item.DateAppointment = data.DateAppointment.Date;
                item.DateCreated = data.DateCreated;
                item.StatusName = _context.StatusTransaction.Where(i => i.IdStatus == data.IdStatus).Single().StatusName;
                item.ReasonOfSick = data.ReasonOfSick;
                itemsTransactionPatient.Add(item);
            }

            vm.ListTransactionPatient = itemsTransactionPatient;
            ViewData["DataSpesialis"] = _context.Spesialis.Where(i => i.Status == "A").ToList();
            if (itemsTransactionPatient.Count > 0)
            {
                vm.NamePatient = itemsTransactionPatient.FirstOrDefault().PatientName;
            }
            else
            {
                vm.NamePatient = "";
            }

            //Doctor below

            vm.UserId = _context.Users.Where(i => i.Email == User.Identity.Name).Single().Id;            
            var user = _context.Users.Where(i => i.UserName == User.Identity.Name).Single().Id;
            //transaction doctor
            var transacationDoctor = from app in _context.AppointmentClinic
                                     join spes in _context.Spesialis on app.IdSpesialis equals spes.Id
                                     join stat in _context.StatusTransaction on app.IdStatus equals stat.IdStatus
                                     where app.Status == "A" && app.UserIdDoctor == user
                                     select new
                                     {
                                         app.IdAppointment,
                                         app.IdSpesialis,
                                         spes.SpesialisName,
                                         Doctor = _context.Users.Where(u => u.Id == app.UserIdDoctor).Single().Name,
                                         Patient = _context.Users.Where(u => u.Id == app.UserIdPatient).Single().Name,
                                         app.Day,
                                         app.TimeAppointment,
                                         app.DateAppointment,
                                         app.IdStatus,
                                         stat.StatusName,
                                         app.ReasonOfSick,
                                         app.DateCreated,
                                         app.UserCreated
                                     };

            foreach (var data in transacationDoctor)
            {
                AppointmentClinicViewModel item = new AppointmentClinicViewModel();
                item.IdAppointment = data.IdAppointment;
                item.Spesialis = data.SpesialisName;
                item.IdStatus = data.IdStatus;
                item.PatientName = data.Patient;
                item.DoctorName = data.Doctor;
                item.Day = data.Day;
                item.TimeAppointment = data.TimeAppointment;
                item.DateAppointment = data.DateAppointment.Date;
                item.DateCreated = data.DateCreated;
                item.StatusName = data.StatusName;
                item.ReasonOfSick = data.ReasonOfSick;
                itemsTransactionDoctor.Add(item);
            }

            if (itemsTransactionDoctor.Count > 0)
            {
                vm.NameDoctor = itemsTransactionDoctor.FirstOrDefault().DoctorName;
            }
            else
            {
                vm.NameDoctor = "";
            }

            vm.ListTransactionDoctor = itemsTransactionDoctor;
            return View(vm);
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
            model.ListSpesialisHours = ListSpesialisHours(model.IdSpesialis);
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