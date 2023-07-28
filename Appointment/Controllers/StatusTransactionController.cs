using Appointment.Models;
using Appointment.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;

namespace Appointment.Controllers
{
    public class StatusTransactionController : Controller
    {
        private readonly AppointmentContext _context;

        public StatusTransactionController(AppointmentContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string sortOrder, string search, int? page)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentFilter"] = search;

            ViewData["Id"] = String.IsNullOrEmpty(sortOrder) ? "uid_d" : "";
            ViewData["Name"] = sortOrder == "name_a" ? "name_d" : "name_a";

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            int totalCount = 0;

            IList<StatusTransactionViewModel> items = new List<StatusTransactionViewModel>();

            var status = from stat in _context.StatusTransaction
                         where stat.Status == "A"
                         select new
                         {
                             stat.IdStatus,
                             stat.StatusName,
                             stat.Status,
                             stat.UserCreated,
                             stat.DateCreated,
                             stat.UserModified,
                             stat.DateModified
                         };

            if (!String.IsNullOrEmpty(search))
            {
                status = status.Where(s => s.StatusName.Contains(search));
            }

            var sortedItems = status.ToList().OrderBy(i => i.StatusName);

            switch (sortOrder)
            {
                case "uid_d":
                    sortedItems = sortedItems.OrderByDescending(i => i.IdStatus);
                    break;
                case "name_a":
                    sortedItems = sortedItems.OrderBy(i => i.StatusName);
                    break;
                case "name_d":
                    sortedItems = sortedItems.OrderByDescending(i => i.StatusName);
                    break;
                case "status_a":
                    sortedItems = sortedItems.OrderBy(i => i.Status);
                    break;
                case "status_d":
                    sortedItems = sortedItems.OrderByDescending(i => i.Status);
                    break;
                default:
                    sortedItems = sortedItems.OrderBy(i => i.IdStatus);
                    break;
            }

            totalCount = status.Count();

            foreach (var itemusr in sortedItems.ToPagedList(pageNumber, pageSize))
            {
                StatusTransactionViewModel item = new StatusTransactionViewModel();
                item.IdStatus = itemusr.IdStatus;
                item.StatusName = itemusr.StatusName;
                item.Status = itemusr.Status;
                item.UserCreated = itemusr.UserCreated;
                item.DateCreated = itemusr.DateCreated;
                item.UserModified = itemusr.UserModified;
                item.DateModified = itemusr.DateModified;

                items.Add(item);
            }

            IPagedList<StatusTransactionViewModel> pagedListData = new StaticPagedList<StatusTransactionViewModel>(items, pageNumber, pageSize, totalCount);
            return View("Index", pagedListData);
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PostCreate(StatusTransactionViewModel model)
        {
            if (ModelState.IsValid)
            {
                StatusTransaction status = new StatusTransaction();
                status.IdStatus = model.IdStatus;
                status.StatusName = model.StatusName;
                status.Status = "A";
                status.DateCreated = DateTime.Now;
                status.UserCreated = User.Identity.Name;
                _context.Add(status);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");

            }
            else
            {
                return View("Create", model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || id == "")
            {
                return NotFound();
            }

            var data = _context.StatusTransaction.Where(i => i.IdStatus == id).Single();

            if (data == null)
            {
                return NotFound();
            }
            else
            {
                StatusTransactionViewModel vm = new StatusTransactionViewModel();
                vm.IdStatus = data.IdStatus;
                vm.StatusName = data.StatusName;
                vm.Status = data.Status;
                vm.UserCreated = data.UserCreated;
                vm.DateCreated = data.DateCreated;
                vm.UserModified = data.UserModified;
                vm.DateModified = data.DateModified;

                return View(vm);
            }            
        }

        [HttpPost]
        public async Task<IActionResult> PostEdit(StatusTransactionViewModel model)
        {
            var edit = _context.StatusTransaction.Where(i => i.IdStatus == model.IdStatus);

            if (edit.Count() > 0)
            {
                var data = edit.Single();
                data.IdStatus = model.IdStatus;
                data.StatusName = model.StatusName;
                data.DateModified = DateTime.Now;
                data.UserModified = User.Identity.Name;
                _context.Update(data);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View("Edit", model);
        }

        [HttpPost]
        public async Task<IActionResult> PostDelete(StatusTransactionViewModel model)
        {
            var edit = _context.StatusTransaction.Where(i => i.IdStatus == model.IdStatus);

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
