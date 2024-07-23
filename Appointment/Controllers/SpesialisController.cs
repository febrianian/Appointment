using Appointment.Models;
using Appointment.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using X.PagedList;

namespace Appointment.Controllers
{
    public class SpesialisController : Controller
    {
        private readonly AppointmentContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        [BindProperty]
        public IFormFile ImageFile { get; set; }
        public SpesialisController(AppointmentContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index(string sortOrder, string search, int? page)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentFilter"] = search;

            ViewData["Id"] = String.IsNullOrEmpty(sortOrder) ? "uid_d" : "";
            ViewData["Name"] = sortOrder == "name_a" ? "name_d" : "name_a";
            ViewData["Status"] = sortOrder == "status_a" ? "status_d" : "status_a";

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            int totalCount = 0;

            IList<SpesialisViewModel> items = new List<SpesialisViewModel>();

            var listUser = _context.Spesialis.ToList();

            if (!String.IsNullOrEmpty(search))
            {
                listUser = listUser.Where(s => s.SpesialisName.Contains(search)).ToList();
            }

            var sortedItems = listUser.ToList().OrderBy(i => i.SpesialisName);

            switch (sortOrder)
            {
                case "uid_d":
                    sortedItems = sortedItems.OrderByDescending(i => i.Id);
                    break;
                case "name_a":
                    sortedItems = sortedItems.OrderBy(i => i.SpesialisName);
                    break;
                case "name_d":
                    sortedItems = sortedItems.OrderByDescending(i => i.SpesialisName);
                    break;                
                default:
                    sortedItems = sortedItems.OrderBy(i => i.Id);
                    break;
            }

            totalCount = listUser.Count();

            foreach (var itemusr in sortedItems.ToPagedList(pageNumber, pageSize))
            {
                SpesialisViewModel item = new SpesialisViewModel();
                item.Id = itemusr.Id;
                item.SpesialisName = itemusr.SpesialisName;
                item.Status = itemusr.Status;

                items.Add(item);
            }

            IPagedList<SpesialisViewModel> pagedListData = new StaticPagedList<SpesialisViewModel>(items, pageNumber, pageSize, totalCount);
            return View("Index", pagedListData);
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        private string UploadedFile(SpesialisViewModel vm)
        {
            string uniqueFileName = null;

            if (vm.files != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + vm.files.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    vm.files.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }

        [HttpPost]
        public async Task<IActionResult> PostCreate(SpesialisViewModel model)
        {            
            Spesialis spesialis = new Spesialis();
            spesialis.SpesialisName = model.SpesialisName;
            spesialis.Description = model.Description;
            spesialis.Status = "A";
            spesialis.DateCreated = DateTime.Now;
            spesialis.UserCreated = User.Identity.Name;
            if (ImageFile != null && ImageFile.Length > 0)
            {
                // Generate a unique filename for the image
                var fileName = $"{Guid.NewGuid().ToString()}_{ImageFile.FileName}";

                // Set the file path to save the image in the "Images" folder inside the "wwwroot" directory
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", fileName);

                // Save the image to the specified path
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                spesialis.ImagesPath = $"/Images/{fileName}";
            }
            else
            {
                spesialis.ImagesPath = "";
            }
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
            vm.Description = data.Description;
            vm.Status = data.Status;
            vm.ImagesPath = data.ImagesPath;
            vm.UserCreated = data.UserCreated;
            vm.DateCreated = data.DateCreated;
            vm.UserModified = data.UserModified;
            vm.DateModified = data.DateModified;
            var status = new List<SelectListItem>();
            status.Add(new SelectListItem { Text = "Active", Value = "A" });
            status.Add(new SelectListItem { Text = "Inactive", Value = "N" });
            ViewData["Status"] = new SelectList(status, "Value", "Text");
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
                data.Description = model.Description;               
                data.Status = model.Status;      
                data.DateModified = DateTime.Now;
                data.UserModified = User.Identity.Name;
                var status = new List<SelectListItem>();
                status.Add(new SelectListItem { Text = "Active", Value = "A" });
                status.Add(new SelectListItem { Text = "Inactive", Value = "N" });
                ViewData["Status"] = new SelectList(status, "Value", "Text", data.Status);
                // Update the image path if a new image is uploaded
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Generate a unique filename for the new image
                    var fileName = $"{Guid.NewGuid().ToString()}_{ImageFile.FileName}";

                    // Set the file path to save the new image in the "Images" folder inside the "wwwroot" directory
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", fileName);

                    // Save the new image to the specified path
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    // Delete the old image file if it exists
                    if (!string.IsNullOrEmpty(data.ImagesPath))
                    {
                        var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, data.ImagesPath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    data.ImagesPath = $"/Images/{fileName}"; // Save the relative path to the new image in the database
                }

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

        [HttpGet]
        public async Task<IActionResult> GetData()
        {
            return View("");
        }
    }
}
