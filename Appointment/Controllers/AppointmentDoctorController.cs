using Appointment.Migrations;
using Appointment.Models;
using Appointment.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MimeKit;
using System.Net;
using X.PagedList;

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
                    //ViewData["Message"] = message;
                    TempData[SD.Warning] = message.ToString();
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
                    message = "Successfully Submited!";
                    //ViewData["Message"] = message;
                    TempData[SD.Success] = message.ToString();
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

        [Authorize(Roles = "Admin")]
        public IActionResult IndexAppointmentAdmin(string sortOrder, string search, int? page)
        {
            ViewData["Id"] = String.IsNullOrEmpty(sortOrder) ? "uid_d" : "";
            ViewData["Patient"] = sortOrder == "patient_a" ? "patient_d" : "patient_a";
            ViewData["Doctor"] = sortOrder == "doctor_a" ? "doctor_d" : "doctor_a";
            ViewData["Spesialis"] = sortOrder == "spesialis_a" ? "spesialis_d" : "spesialis_a";
            ViewData["Status"] = sortOrder == "status_a" ? "status_d" : "status_a";
            ViewData["Day"] = sortOrder == "day_a" ? "day_d" : "day_a";
            ViewData["DateCreated"] = sortOrder == "datecreated_a" ? "datecreated_d" : "datecreated_a";

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            int totalCount = 0;

            IList<AppointmentClinicViewModel> items = new List<AppointmentClinicViewModel>();
            var list = from app in _context.AppointmentClinic
                       join spes in _context.Spesialis on app.IdSpesialis equals spes.Id
                       join userP in _context.Users on app.UserIdPatient equals userP.Id
                       join userD in _context.Users on app.UserIdDoctor equals userD.Id
                       join stat in _context.StatusTransaction on app.IdStatus equals stat.IdStatus
                       where app.Status == "A"
                       select new
                       {
                           app.IdAppointment,
                           app.UserModified,
                           app.UserCreated,
                           app.DateModified,
                           app.DateCreated,
                           spes.Id,
                           spes.SpesialisName,
                           Patient = userP.Name,
                           Doctor = userD.Name,
                           app.TimeAppointment,
                           app.DateAppointment,
                           app.Day,
                           app.ReasonOfSick,
                           stat.IdStatus,
                           stat.StatusName
                       };

            if (!String.IsNullOrEmpty(search))
            {
                list = list.Where(s => s.SpesialisName.Contains(search)
                || s.Patient.Contains(search)
                || s.Doctor.Contains(search)
                || s.Day.Contains(search)
                || s.ReasonOfSick.Contains(search));
            }

            var sortedItems = list.ToList().OrderBy(i => i.IdAppointment);

            switch (sortOrder)
            {
                case "uid_d":
                    sortedItems = sortedItems.OrderByDescending(i => i.IdAppointment);
                    break;
                case "patient_a":
                    sortedItems = sortedItems.OrderBy(i => i.Patient);
                    break;
                case "patient_d":
                    sortedItems = sortedItems.OrderByDescending(i => i.Patient);
                    break;
                case "doctor_a":
                    sortedItems = sortedItems.OrderBy(i => i.Doctor);
                    break;
                case "doctor_d":
                    sortedItems = sortedItems.OrderByDescending(i => i.Doctor);
                    break;
                case "spesialis_a":
                    sortedItems = sortedItems.OrderBy(i => i.SpesialisName);
                    break;
                case "spesialis_d":
                    sortedItems = sortedItems.OrderByDescending(i => i.SpesialisName);
                    break;
                case "status_a":
                    sortedItems = sortedItems.OrderBy(i => i.StatusName);
                    break;
                case "status_d":
                    sortedItems = sortedItems.OrderByDescending(i => i.StatusName);
                    break;
                case "day_a":
                    sortedItems = sortedItems.OrderBy(i => i.Day);
                    break;
                case "day_d":
                    sortedItems = sortedItems.OrderByDescending(i => i.Day);
                    break;
                case "datecreated_a":
                    sortedItems = sortedItems.OrderBy(i => i.DateCreated);
                    break;
                case "datecreated_d":
                    sortedItems = sortedItems.OrderByDescending(i => i.DateCreated);
                    break;
                default:
                    sortedItems = sortedItems.OrderBy(i => i.IdAppointment);
                    break;
            }

            foreach (var item in list)
            {
                AppointmentClinicViewModel vm = new AppointmentClinicViewModel();
                vm.IdAppointment = item.IdAppointment;
                vm.DateAppointment= item.DateAppointment;
                vm.Spesialis = item.SpesialisName;
                vm.TimeAppointment = item.TimeAppointment;
                vm.Day = item.Day;
                vm.ReasonOfSick = item.ReasonOfSick;
                vm.StatusName = item.StatusName;
                vm.DateCreated = item.DateCreated;
                vm.DoctorName = item.Doctor;
                vm.PatientName = item.Patient;
                items.Add(vm);
            }

            IPagedList<AppointmentClinicViewModel> pagedListData = new StaticPagedList<AppointmentClinicViewModel>(items, pageNumber, pageSize, totalCount);
            return View("IndexAppointmentAdmin", pagedListData);
        }

        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Proccess(int idAppointment)
        {
            var transaction = (from app in _context.AppointmentClinic
                                     join spes in _context.Spesialis on app.IdSpesialis equals spes.Id
                                     join stat in _context.StatusTransaction on app.IdStatus equals stat.IdStatus
                                     where app.Status == "A" && app.IdAppointment == idAppointment
                                     select new
                                     {
                                         app.IdAppointment,
                                         app.IdSpesialis,
                                         spes.SpesialisName,
                                         app.HistoryOfSick,
                                         app.Age,
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
                                     }).Single();

            AppointmentClinicViewModel vm = new AppointmentClinicViewModel();
            vm.IdAppointment = transaction.IdAppointment;
            vm.Spesialis = transaction.SpesialisName;
            vm.PatientName = transaction.Patient;
            vm.Day = transaction.Day;
            vm.Age = transaction.Age;
            vm.TimeAppointment = transaction.TimeAppointment;
            vm.DateAppointment = transaction.DateAppointment;
            vm.StatusName = transaction.StatusName;
            vm.ReasonOfSick = transaction.ReasonOfSick;
            vm.HistoryOfSick = transaction.HistoryOfSick;
            vm.DateCreated = transaction.DateCreated;
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveRequest(AppointmentClinicViewModel vm)
        {
            var message = "";

            var transaction = _context.AppointmentClinic.Where(i => i.IdAppointment == vm.IdAppointment).Single();

            //update status
            transaction.IdStatus = "4";
            transaction.DateModified = DateTime.Now;
            transaction.UserModified = User.Identity.Name;
            _context.Update(transaction);
            _context.SaveChanges();
           
            var spesialis = _context.Spesialis.Where(x => x.Id == transaction.IdSpesialis).Single().SpesialisName;
            string subject = "Appoitment " + spesialis;
            string htmlBody = "Details are as follows:<br/><br/>";
            htmlBody += "<table>";
            htmlBody += "<tr><td>Spesialis</td><td>: " + spesialis + "</td></tr>";
            htmlBody += "<tr><td>Doctor</td><td>: " + _context.Users.Where(i => i.Id == transaction.UserIdDoctor).Single().Name + "</td></tr>";
            htmlBody += "<tr><td>Tanggal Konsultasi</td><td>: " + transaction.DateAppointment.Date.ToString("dd MMMM yyyy") + "</td></tr>";
            htmlBody += "<tr><td>Hari / Waktu</td><td>: " + transaction.Day + " / " + transaction.TimeAppointment + "</td></tr>";
            htmlBody += "<tr><td>Keluhan</td><td>: " + transaction.ReasonOfSick + "</td></tr>";
            htmlBody += "<tr><td>Status</td><td>: " + _context.StatusTransaction.Where(i => i.IdStatus == transaction.IdStatus).Single().StatusName + "</td></tr>";
            htmlBody += "</table><br/><br/>";
            htmlBody += "<br/><br/>";
            htmlBody += "<center><small><b><i>This email is generated automatically by system.<br/>Please do not reply to this email.</i></b></small></center>";

            string from = "Appointment Clinic";
            string status = "Success";
            string toTitle = transaction.UserCreated;
            string toEmail = transaction.UserCreated;
            transaction.DoctorNote = vm.DoctorName;
            await SentEmail(subject, htmlBody, status, from, true, toTitle, toEmail);
            message = "Successfully Submited!";
            //ViewData["Message"] = message;
            TempData[SD.Success] = message.ToString();

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> RejectRequest(AppointmentClinicViewModel vm)
        {
            var message = "";
            var transaction = _context.AppointmentClinic.Where(i => i.IdAppointment == vm.IdAppointment).Single();

            //update status
            transaction.IdStatus = "3";
            transaction.DateModified = DateTime.Now;
            transaction.UserModified = User.Identity.Name;
            transaction.DoctorNote = vm.DoctorName;
            _context.Update(transaction);
            _context.SaveChanges();

            var spesialis = _context.Spesialis.Where(x => x.Id == transaction.IdSpesialis).Single().SpesialisName;
            string subject = "Appoitment " + spesialis;
            string htmlBody = "Details are as follows:<br/><br/>";
            htmlBody += "<table>";
            htmlBody += "<tr><td>Spesialis</td><td>: " + spesialis + "</td></tr>";
            htmlBody += "<tr><td>Doctor</td><td>: " + _context.Users.Where(i => i.Id == transaction.UserIdDoctor).Single().Name + "</td></tr>";
            htmlBody += "<tr><td>Tanggal Konsultasi</td><td>: " + transaction.DateAppointment.Date.ToString("dd MMMM yyyy") + "</td></tr>";
            htmlBody += "<tr><td>Hari / Waktu</td><td>: " + transaction.Day + " / " + transaction.TimeAppointment + "</td></tr>";
            htmlBody += "<tr><td>Keluhan</td><td>: " + transaction.ReasonOfSick + "</td></tr>";
            htmlBody += "<tr><td>Status</td><td>: " + _context.StatusTransaction.Where(i => i.IdStatus == transaction.IdStatus).Single().StatusName + "</td></tr>";
            htmlBody += "</table><br/><br/>";
            htmlBody += "<br/><br/>";
            htmlBody += "<center><small><b><i>This email is generated automatically by system.<br/>Please do not reply to this email.</i></b></small></center>";

            string from = "Appointment Clinic";
            string status = "Success";
            string toTitle = transaction.UserCreated;
            string toEmail = transaction.UserCreated;

            await SentEmail(subject, htmlBody, status, from, true, toTitle, toEmail);
            message = "Successfully Submited!";
            //ViewData["Message"] = message;
            TempData[SD.Success] = message.ToString();

            return RedirectToAction("Index", "Home");
        }
        
        public async Task<IActionResult> DetailsAppointment(int idAppointment)
        {
            var transaction = (from app in _context.AppointmentClinic
                               join spes in _context.Spesialis on app.IdSpesialis equals spes.Id
                               join stat in _context.StatusTransaction on app.IdStatus equals stat.IdStatus
                               where app.Status == "A" && app.IdAppointment == idAppointment
                               select new
                               {
                                   app.IdAppointment,
                                   app.IdSpesialis,
                                   spes.SpesialisName,
                                   app.HistoryOfSick,
                                   app.DoctorNote,
                                   app.Age,
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
                               }).Single();

            AppointmentClinicViewModel vm = new AppointmentClinicViewModel();
            vm.IdAppointment = transaction.IdAppointment;
            vm.Spesialis = transaction.SpesialisName;
            vm.PatientName = transaction.Patient;
            vm.Day = transaction.Day;
            vm.Age = transaction.Age;
            vm.TimeAppointment = transaction.TimeAppointment;
            vm.DateAppointment = transaction.DateAppointment;
            vm.StatusName = transaction.StatusName;
            vm.ReasonOfSick = transaction.ReasonOfSick;
            vm.HistoryOfSick = transaction.HistoryOfSick;
            vm.DateCreated = transaction.DateCreated;
            vm.DoctorNote = transaction.DoctorNote;
            return View(vm);
        }
    }
}
