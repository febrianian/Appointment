using Appointment.Models;
using Appointment.Services;
using Appointment.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;

namespace Appointment.Controllers
{
    public class SpesialisScheduleController : Controller
    {
        private readonly AppointmentContext _context;
        private readonly IAppointmentService _appointmentService;

        public SpesialisScheduleController(AppointmentContext context, IAppointmentService appointmentService)
        {
            _context = context;
            _appointmentService = appointmentService;
        }
        public async Task<IActionResult> Index(string sortOrder, string search, int? page)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentFilter"] = search;

            ViewData["Id"] = String.IsNullOrEmpty(sortOrder) ? "uid_d" : "";
            ViewData["Name"] = sortOrder == "name_a" ? "name_d" : "name_a";
            ViewData["Day"] = sortOrder == "name_a" ? "name_d" : "name_a";

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            int totalCount = 0;

            IList<SpesialisScheduleViewModel> items = new List<SpesialisScheduleViewModel>();

            var schedule = from sche in _context.SpesialisSchedule
                           join spes in _context.Spesialis on sche.IdSpesialis equals spes.Id
                           where sche.Status == "A"
                           select new
                           {
                               sche.IdSpesialisSchedule,
                               spes.SpesialisName,
                               sche.ScheduleDay,
                               sche.StartDate,
                               sche.EndDate
                           };

            if (!String.IsNullOrEmpty(search))
            {
                schedule = schedule.Where(s => s.SpesialisName.Contains(search)
                || s.ScheduleDay.Contains(search));
            }

            var sortedItems = schedule.ToList().OrderBy(i => i.ScheduleDay);

            switch (sortOrder)
            {
                case "uid_d":
                    sortedItems = sortedItems.OrderByDescending(i => i.IdSpesialisSchedule);
                    break;
                case "usr_a":
                    sortedItems = sortedItems.OrderBy(i => i.ScheduleDay);
                    break;
                case "usr_d":
                    sortedItems = sortedItems.OrderByDescending(i => i.ScheduleDay);
                    break;
                default:
                    sortedItems = sortedItems.OrderBy(i => i.IdSpesialisSchedule);
                    break;
            }

            totalCount = schedule.Count();

            foreach (var itemusr in sortedItems.ToPagedList(pageNumber, pageSize))
            {
                SpesialisScheduleViewModel item = new SpesialisScheduleViewModel();
                item.IdSpesialisSchedule = itemusr.IdSpesialisSchedule;
                item.SpesialisName = itemusr.SpesialisName;
                item.ScheduleDay = itemusr.ScheduleDay;
                item.StartDate = itemusr.StartDate.ToString("HH:mm");
                item.EndDate = itemusr.EndDate.ToString("HH:mm");

                items.Add(item);
            }

            IPagedList<SpesialisScheduleViewModel> pagedListData = new StaticPagedList<SpesialisScheduleViewModel>(items, pageNumber, pageSize, totalCount);
            return View("Index", pagedListData);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.DoctorList = _appointmentService.GetDoctorList();

            var spesialis =
                  _context.Spesialis
                  .Where(i => i.Status == "A")
                  .Select(t => new
                  {
                      Id = t.Id,
                      SpesialisName = t.SpesialisName
                  });

            ViewData["Spesialist"] = new SelectList(spesialis, "Id", "SpesialisName");

            var dayList = new List<SelectListItem>();
            dayList.Add(new SelectListItem { Text = "Senin", Value = "Senin" });
            dayList.Add(new SelectListItem { Text = "Selasa", Value = "Selasa" });
            dayList.Add(new SelectListItem { Text = "Rabu", Value = "Rabu" });
            dayList.Add(new SelectListItem { Text = "Kamis", Value = "Kamis" });
            dayList.Add(new SelectListItem { Text = "Jumat", Value = "Jumat" });
            dayList.Add(new SelectListItem { Text = "Sabtu", Value = "Sabtu" });
            dayList.Add(new SelectListItem { Text = "Minggu", Value = "Minggu" });
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
            string endDate = model.StartDate;
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
            var data = _context.Spesialis.Where(i => i.Id == id).Single();

            SpesialisViewModel vm = new SpesialisViewModel();
            vm.Id = data.Id;
            vm.SpesialisName = data.SpesialisName;
            vm.Status = data.Status;
            vm.UserCreated = data.UserCreated;
            vm.DateCreated = data.DateCreated;
            vm.UserModified = data.UserModified;
            vm.DateModified = data.DateModified;

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> PostEdit(SpesialisViewModel model)
        {
            var edit = _context.Spesialis.Where(i => i.Id == model.Id);

            if (edit.Count() > 0)
            {
                var data = edit.Single();
                data.SpesialisName = model.SpesialisName.Trim();
                data.DateModified = DateTime.Now;
                data.UserModified = User.Identity.Name;
                _context.Update(data);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View("Edit", model);
        }

        [HttpPost]
        public async Task<IActionResult> PostDelete(SpesialisViewModel model)
        {
            var edit = _context.Spesialis.Where(i => i.Id == model.Id);

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
