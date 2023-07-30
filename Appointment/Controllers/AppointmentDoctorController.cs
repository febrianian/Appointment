using Appointment.Migrations;
using Appointment.Models;
using Appointment.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Appointment.Controllers
{
    public class AppointmentDoctorController : Controller
    {
        private readonly AppointmentContext _context;

        public AppointmentDoctorController(AppointmentContext context)
        {
            _context = context;
        }
        public IActionResult Index(AppointmentDoctorViewModel vm, int idSpesialis, string userId)
        {
            int minute = 60;
            List<SelectListItem> duration = new List<SelectListItem>();

            for (int i = 1; i <= 24; i++)
            {
                duration.Add(new SelectListItem { Value = (i + ":00").ToString(), Text = i + ":00" });
                minute = minute + 30;
                duration.Add(new SelectListItem { Value = (i + ":00").ToString(), Text = i + ":30" });
                minute = minute + 30;
            }

            ViewData["Hours"] = new SelectList(duration, "Value", "Text");

            var doctor = _context.Users.Where(u => u.Id == userId).Single();
            var user = _context.Users.Where(u => u.UserName == User.Identity.Name).Single();
            vm.IdSpesialis = idSpesialis;
            vm.UserId = userId;
            vm.DoctorName = doctor.Name;
            vm.Spesialis = _context.Spesialis.Where(i => i.Id == idSpesialis).Single().SpesialisName;
            vm.Name = user.Name;
            vm.Email = user.Email;
            vm.PhoneNumber = user.PhoneNumber;
            vm.Address = user.Address;

            DateTime currentDate = DateTime.Today;
            int age = currentDate.Year - user.BirthDate.Year;
            
            if (user.BirthDate.Date > currentDate.AddYears(-age))
            {
                age--;
            }

            vm.Age = age.ToString() + " Tahun";

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> MakeAppointment(AppointmentDoctorViewModel vm)
        {
            var message = "";
            DateTime inputDateTime = vm.DateAppointment.Date;
            DayOfWeek dayOfWeek = inputDateTime.DayOfWeek;
            string dayOfWeekString = GetBahasaIndonesiaDayOfWeek(dayOfWeek);            

            var schedule = _context.SpesialisSchedule.Where(i => i.IdSpesialis == vm.IdSpesialis && i.UserId == vm.UserId && i.Status == "A");
            DateTime selectTime = DateTime.ParseExact(vm.TimeAppointment, "H:mm", System.Globalization.CultureInfo.InvariantCulture);
            int hour = selectTime.Hour;
            var scheduleDay = schedule.Where(i => i.ScheduleDay == dayOfWeekString).Single().ScheduleDay;

            var doctor = _context.Users.Where(u => u.Id == vm.UserId).Single();
            var user = _context.Users.Where(u => u.UserName == User.Identity.Name).Single();

            //check if day are same
            if (dayOfWeekString == scheduleDay)
            {
                //get from schedule if time already exists
                var time = schedule.Where(i => i.ScheduleDay == dayOfWeekString).Single();
                DateTime startTime = time.StartDate; //2023-07-24 06:00:00.000000
                DateTime endTime = time.EndDate; //2023-07-24 16:00:00.000000
                int timeStartHour = startTime.Hour; // 6
                int timeEndHour = endTime.Hour; // 16              

                if (hour < timeStartHour || hour > timeEndHour) 
                {
                    message = "Your time out of selection range";
                    ViewData["Message"] = message;

                    int minute = 60;
                    List<SelectListItem> duration = new List<SelectListItem>();

                    for (int i = 1; i <= 24; i++)
                    {
                        duration.Add(new SelectListItem { Value = (i + ":00").ToString(), Text = i + ":00" });
                        minute = minute + 30;
                        duration.Add(new SelectListItem { Value = (i + ":00").ToString(), Text = i + ":30" });
                        minute = minute + 30;
                    }

                    ViewData["Hours"] = new SelectList(duration, "Value", "Text");

                    return View("Index", vm);
                }

                AppointmentClinic model = new AppointmentClinic();

                using(var transSql = _context.Database.BeginTransaction()) 
                {
                    model.IdSpesialis = vm.IdSpesialis;
                    model.UserIdPatient = user.Id;
                    model.UserIdDoctor = doctor.Id;
                    model.Age = vm.Age;
                    model.DateAppointment = vm.DateAppointment.Date;
                    model.Day = dayOfWeekString;
                    model.TimeAppointment = hour.ToString("H:mm");
                    model.ReasonOfSick = vm.ReasonOfSick;
                    model.HistoryOfSick = vm.HistoryOfSick;
                    model.IdStatus = "1";
                    model.Status = "A";
                    model.DateCreated = DateTime.Now;
                    model.UserCreated = User.Identity.Name;

                    _context.Add(model);
                    await _context.SaveChangesAsync();
                    transSql.Commit();

                    return RedirectToAction("Index", "Home");
                }
            }

            return View(vm);
        }

        static string GetBahasaIndonesiaDayOfWeek(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return "Minggu";
                case DayOfWeek.Monday:
                    return "Senin";
                case DayOfWeek.Tuesday:
                    return "Selasa";
                case DayOfWeek.Wednesday:
                    return "Rabu";
                case DayOfWeek.Thursday:
                    return "Kamis";
                case DayOfWeek.Friday:
                    return "Jumat";
                case DayOfWeek.Saturday:
                    return "Sabtu";
                default:
                    throw new ArgumentOutOfRangeException(nameof(dayOfWeek), "Invalid day of the week.");
            }
        }
    }
}
