using Appointment.Migrations;
using Appointment.Models;
using Appointment.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MimeKit;
using System.Net;

namespace Appointment.Controllers
{
    public class AppointmentDoctorController : Controller
    {
        private readonly AppointmentContext _context;
        private readonly IConfiguration _config;

        public AppointmentDoctorController(AppointmentContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        public async Task SentEmail(string subject, string htmlBody, string status, string from, bool show, string toAddressTitle, string toAddress)
        {
            //Check configuration
            var dev = _config["EmailSettings:DeveloperMode"];
            //send Email Here
            string FromAddress = _config["EmailSettings:SenderEmail"];
            string FromAdressTitle = _config["EmailSettings:SenderName"];
            string ToAddress = "";
            string ToAddressTitle = "";

            var bodyBuilder = new BodyBuilder();
            var mimeMessage = new MimeMessage();
            ToAddress = "";
            ToAddressTitle = "";

            string Subject = subject;
            bodyBuilder.HtmlBody = htmlBody;
            mimeMessage.From.Add(new MailboxAddress(FromAdressTitle, FromAddress));
            mimeMessage.Subject = Subject;
            mimeMessage.Body = bodyBuilder.ToMessageBody();
            ToAddressTitle = toAddressTitle;
            ToAddress = toAddress;

            if (dev == "false")
            {
                mimeMessage.To.Add(new MailboxAddress(toAddressTitle, toAddress));
            }
            else if (dev == "true")
            {
                mimeMessage.To.Add(new MailboxAddress("febrian.evolution@gmail.com", "febrian.evolution@gmail.com"));
            }

            //Check configuration
            var serverAddress = _config["EmailSettings:SmtpServer"];
            var emailPort = _config["EmailSettings:Port"];
            var emailUsername = _config["EmailSettings:Username"];
            var emailPass = _config["EmailSettings:Password"];
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                try
                {
                    client.Connect(serverAddress, Convert.ToInt32(emailPort), false);
                    client.Authenticate(emailUsername, emailPass);
                    client.Send(mimeMessage);
                    client.Disconnect(true);
                }
                catch (Exception ex)
                {
                    return;
                }
                finally
                {
                    if (client.IsConnected == true)
                    {
                        client.Disconnect(true);
                    }
                }
            }
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

                if (hour < timeStartHour && hour > timeEndHour) 
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
                    model.TimeAppointment = vm.TimeAppointment;
                    model.ReasonOfSick = vm.ReasonOfSick;
                    model.HistoryOfSick = vm.HistoryOfSick;
                    model.IdStatus = "1";
                    model.Status = "A";
                    model.DateCreated = DateTime.Now;
                    model.UserCreated = User.Identity.Name;

                    _context.Add(model);
                    await _context.SaveChangesAsync();
                    transSql.Commit();

                    string subject = "Appoitment Successfully Submited !";
                    string htmlBody = "Details are as follows:<br/><br/>";

                    var spesialis = _context.Spesialis.Where(x => x.Id == model.IdSpesialis).Single().SpesialisName;

                    htmlBody += "<table>";
                    htmlBody += "<tr><td>Spesialis</td><td>: " + spesialis + "</td></tr>";
                    htmlBody += "<tr><td>Doctor</td><td>: " + doctor.Name + "</td></tr>";
                    htmlBody += "<tr><td>Tanggal Konsultasi</td><td>: " + model.DateAppointment.Date.ToString("dd MMMM yyyy") + "</td></tr>";
                    htmlBody += "<tr><td>Hari / Waktu</td><td>: " + model.Day + " / " + model.TimeAppointment + "</td></tr>";
                    htmlBody += "<tr><td>Keluhan</td><td>: " + model.ReasonOfSick + "</td></tr>";                    
                    htmlBody += "</table><br/><br/>";
                    htmlBody += "<br/><br/>";
                    htmlBody += "Mohon untuk datang minimal 15 menit sebelum waktu yang sudah ditentukan, terima kasih.";
                    htmlBody += "<center><small><b><i>This email is generated automatically by system.<br/>Please do not reply to this email.</i></b></small></center>";

                    string from = "Appointment Clinic";
                    string status = "Success";
                    string toTitle = user.Email;
                    string toEmail = user.Email;

                    await SentEmail(subject, htmlBody, status, from, true, toTitle, toEmail);

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
