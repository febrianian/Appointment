using Appointment.Models;
using Appointment.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;

namespace Appointment.Controllers
{
    public class SpesialisScheduleController : Controller
    {
        private readonly AppointmentContext _context;

        public SpesialisScheduleController(AppointmentContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string sortOrder, string search, int? page)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentFilter"] = search;

            ViewData["Id"] = String.IsNullOrEmpty(sortOrder) ? "uid_d" : "";
            ViewData["Name"] = sortOrder == "name_a" ? "name_d" : "name_a";
            ViewData["Spesialis"] = sortOrder == "spesialis_a" ? "spesialis_d" : "spesialis_a";
            ViewData["Status"] = sortOrder == "status_a" ? "status_d" : "status_a";
            ViewData["Day"] = sortOrder == "day_a" ? "day_d" : "day_a";

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            int totalCount = 0;

            IList<SpesialisScheduleViewModel> items = new List<SpesialisScheduleViewModel>();

            var schedule = from sche in _context.SpesialisSchedule
                           join spes in _context.Spesialis on sche.IdSpesialis equals spes.Id
                           join user in _context.Users on sche.UserId equals user.Id                           
                           select new
                           {
                               sche.IdSpesialisSchedule,
                               spes.SpesialisName,
                               sche.Status,
                               user.Name,
                               sche.ScheduleDay,
                               sche.StartDate,
                               sche.EndDate
                           };

            if (!String.IsNullOrEmpty(search))
            {
                schedule = schedule.Where(s => s.SpesialisName.Contains(search)
                || s.ScheduleDay.Contains(search)
                || s.Name.Contains(search));
            }

            var sortedItems = schedule.ToList().OrderBy(i => i.ScheduleDay);

            switch (sortOrder)
            {
                case "uid_d":
                    sortedItems = sortedItems.OrderByDescending(i => i.IdSpesialisSchedule);
                    break;
                case "name_a":
                    sortedItems = sortedItems.OrderBy(i => i.Name);
                    break;
                case "name_d":
                    sortedItems = sortedItems.OrderByDescending(i => i.Name);
                    break;
                case "spesialis_a":
                    sortedItems = sortedItems.OrderBy(i => i.SpesialisName);
                    break;
                case "spesialis_d":
                    sortedItems = sortedItems.OrderByDescending(i => i.SpesialisName);
                    break;
                case "status_a":
                    sortedItems = sortedItems.OrderBy(i => i.Status);
                    break;
                case "status_d":
                    sortedItems = sortedItems.OrderByDescending(i => i.Status);
                    break;
                case "day_a":
                    sortedItems = sortedItems.OrderBy(i => i.ScheduleDay);
                    break;
                case "day_d":
                    sortedItems = sortedItems.OrderByDescending(i => i.ScheduleDay);
                    break;
                default:
                    sortedItems = sortedItems.OrderBy(i => i.IdSpesialisSchedule);
                    break;
            }

            totalCount = schedule.Count();

            foreach (var itemusr in sortedItems.ToPagedList(pageNumber, pageSize))
            {
                var dayName = itemusr.ScheduleDay;

                if(dayName == "Monday")
                {
                    dayName = "Senin";
                }
                else if (dayName == "Tuesday")
                {
                    dayName = "Selasa";
                }
                else if (dayName == "Wednesday")
                {
                    dayName = "Rabu";
                }
                else if (dayName == "Thursday")
                {
                    dayName = "Kamis";
                }
                else if (dayName == "Friday")
                {
                    dayName = "Jum'at";
                }
                else if (dayName == "Saturday")
                {
                    dayName = "Sabtu";
                }
                else
                {
                    dayName = "Minggu";
                }

                SpesialisScheduleViewModel item = new SpesialisScheduleViewModel();
                item.IdSpesialisSchedule = itemusr.IdSpesialisSchedule;
                item.Name = itemusr.Name;
                item.Status = itemusr.Status;
                item.SpesialisName = itemusr.SpesialisName;
                item.ScheduleDay = dayName;
                item.StartDate = itemusr.StartDate.ToString("HH:mm");
                item.EndDate = itemusr.EndDate.ToString("HH:mm");

                items.Add(item);
            }

            IPagedList<SpesialisScheduleViewModel> pagedListData = new StaticPagedList<SpesialisScheduleViewModel>(items, pageNumber, pageSize, totalCount);
            return View("Index", pagedListData);
        }

        public async Task<IActionResult> Create()
        {            
            var spesialis =
                  _context.Spesialis
                  .Where(i => i.Status == "A")
                  .Select(t => new
                  {
                      Id = t.Id,
                      SpesialisName = t.SpesialisName
                  });

            ViewData["Spesialist"] = new SelectList(spesialis, "Id", "SpesialisName");
            
            var doctors = (from user in _context.Users
                           join userRoles in _context.UserRoles on user.Id equals userRoles.UserId
                           join roles in _context.Roles on userRoles.RoleId equals roles.Id
                           where roles.Name == "Doctor"
                           select new DoctorVM
                           {
                               Id = user.Id,
                               Name = user.Name
                           }
                           ).ToList();

            var doctor = doctors
                          .Select(t => new
                          {
                              Id = t.Id,
                              Name = t.Name
                          });


            ViewData["DoctorList"] = new SelectList(doctor, "Id", "Name");

            var dayList = new List<SelectListItem>();
            dayList.Add(new SelectListItem { Text = "Senin", Value = "Monday" });
            dayList.Add(new SelectListItem { Text = "Selasa", Value = "Tuesday" });
            dayList.Add(new SelectListItem { Text = "Rabu", Value = "Wednesday" });
            dayList.Add(new SelectListItem { Text = "Kamis", Value = "Thursday" });
            dayList.Add(new SelectListItem { Text = "Jumat", Value = "Friday" });
            dayList.Add(new SelectListItem { Text = "Sabtu", Value = "Saturday" });
            dayList.Add(new SelectListItem { Text = "Minggu", Value = "Sunday" });
            ViewData["DayList"] = new SelectList(dayList, "Value", "Text");

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

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PostCreate(SpesialisScheduleViewModel model)
        {
            string startDate = model.StartDate;
            DateTime startDateModify = DateTime.ParseExact(startDate, "H:mm", null);
            DateTime updatedStartDate = new DateTime(startDateModify.Year, startDateModify.Month, startDateModify.Day, startDateModify.Hour, 0, 0);
            string endDate = model.EndDate;
            DateTime endDateModify = DateTime.ParseExact(endDate, "H:mm", null);
            DateTime updatedEndDate = new DateTime(endDateModify.Year, endDateModify.Month, endDateModify.Day, endDateModify.Hour, 0, 0);

            SpesialisSchedule spesialis = new SpesialisSchedule();
            spesialis.IdSpesialis = model.IdSpesialis;
            spesialis.UserId = model.UserId;
            spesialis.ScheduleDay = model.ScheduleDay;
            spesialis.StartDate = updatedStartDate;
            spesialis.EndDate = updatedEndDate;
            spesialis.Status = "A";
            spesialis.DateCreated = DateTime.Now;
            spesialis.UserCreated = User.Identity.Name;
            _context.Add(spesialis);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id)
        {
            var data = _context.SpesialisSchedule.Where(i => i.IdSpesialisSchedule == id).Single();

            SpesialisScheduleViewModel vm = new SpesialisScheduleViewModel();
            vm.IdSpesialisSchedule = data.IdSpesialisSchedule;
            vm.Status = data.Status;
            vm.UserCreated = data.UserCreated;
            vm.DateCreated = data.DateCreated;
            vm.UserModified = data.UserModified;
            vm.DateModified = data.DateModified;

            var spesialis =
                  _context.Spesialis
                  .Where(i => i.Status == "A")
                  .Select(t => new
                  {
                      Id = t.Id,
                      SpesialisName = t.SpesialisName
                  });

            ViewData["Spesialist"] = new SelectList(spesialis, "Id", "SpesialisName", data.IdSpesialis);

            var doctors = (from user in _context.Users
                           join userRoles in _context.UserRoles on user.Id equals userRoles.UserId
                           join roles in _context.Roles on userRoles.RoleId equals roles.Id
                           where roles.Name == "Doctor"
                           select new DoctorVM
                           {
                               Id = user.Id,
                               Name = user.Name
                           }
                           ).ToList();

            var doctor = doctors
                          .Select(t => new
                          {
                              Id = t.Id,
                              Name = t.Name
                          });

            ViewData["DoctorList"] = new SelectList(doctor, "Id", "Name", data.UserId);

            var dayList = new List<SelectListItem>();
            dayList.Add(new SelectListItem { Text = "Senin", Value = "Senin" });
            dayList.Add(new SelectListItem { Text = "Selasa", Value = "Selasa" });
            dayList.Add(new SelectListItem { Text = "Rabu", Value = "Rabu" });
            dayList.Add(new SelectListItem { Text = "Kamis", Value = "Kamis" });
            dayList.Add(new SelectListItem { Text = "Jumat", Value = "Jumat" });
            dayList.Add(new SelectListItem { Text = "Sabtu", Value = "Sabtu" });
            dayList.Add(new SelectListItem { Text = "Minggu", Value = "Minggu" });
            ViewData["DayList"] = new SelectList(dayList, "Value", "Text", data.ScheduleDay);

            int minute = 60;
            List<SelectListItem> duration = new List<SelectListItem>();

            for (int i = 1; i <= 24; i++)
            {
                duration.Add(new SelectListItem { Value = (i + ":00").ToString(), Text = i + ":00" });
                minute = minute + 30;
                duration.Add(new SelectListItem { Value = (i + ":00").ToString(), Text = i + ":30" });
                minute = minute + 30;
            }

            ViewData["Hours1"] = new SelectList(duration, "Value", "Text", data.StartDate);
            ViewData["Hours2"] = new SelectList(duration, "Value", "Text", data.EndDate);

            var status = new List<SelectListItem>();
            status.Add(new SelectListItem { Text = "Active", Value = "A" });
            status.Add(new SelectListItem { Text = "Inactive", Value = "N" });
            ViewData["Status"] = new SelectList(status, "Value", "Text");

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> PostEdit(SpesialisScheduleViewModel model)
        {
            var edit = _context.SpesialisSchedule.Where(i => i.IdSpesialisSchedule == model.IdSpesialisSchedule);

            string startDate = model.StartDate;
            DateTime startDateModify = DateTime.ParseExact(startDate, "H:mm", null);
            DateTime updatedStartDate = new DateTime(startDateModify.Year, startDateModify.Month, startDateModify.Day, startDateModify.Hour, 0, 0);
            string endDate = model.EndDate;
            DateTime endDateModify = DateTime.ParseExact(endDate, "H:mm", null);
            DateTime updatedEndDate = new DateTime(endDateModify.Year, endDateModify.Month, endDateModify.Day, endDateModify.Hour, 0, 0);

            if (edit.Count() > 0)
            {
                var data = edit.Single();
                data.IdSpesialis = model.IdSpesialis;
                data.UserId = model.UserId;
                data.Status = model.Status;
                data.StartDate = updatedStartDate;
                data.EndDate = updatedEndDate;
                data.DateModified = DateTime.Now;
                data.UserModified = User.Identity.Name;
                _context.Update(data);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View("Edit", model);
        }

        [HttpPost]
        public async Task<IActionResult> PostDelete(SpesialisScheduleViewModel model)
        {
            var edit = _context.SpesialisSchedule.Where(i => i.IdSpesialisSchedule == model.IdSpesialisSchedule);

            if (edit.Count() > 0)
            {
                var data = edit.Single();
                data.Status = "N";
                data.DateModified = DateTime.Now;
                data.UserModified = User.Identity.Name;
                _context.Update(data);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View("Edit", model);
        }
    }
}
