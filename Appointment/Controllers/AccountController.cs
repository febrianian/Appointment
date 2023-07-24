using Appointment.ViewModels;
using Appointment.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using MimeKit;
using System;

namespace Appointment.Controllers
{
    //[Authorize(Roles = "Appointment-SuperUser")]
    public class AccountController : Controller
    {
        private readonly AppointmentContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _singInManager;
        private readonly IWebHostEnvironment _webEnvirontment;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;
        private string active = "active";
        private string parent = "menu-open";

        public AccountController(UserManager<ApplicationUser> userManager,IConfiguration config, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager, AppointmentContext context, IWebHostEnvironment webEnvirontment)
        {
            _context = context;
            _userManager = userManager;
            _singInManager = signInManager;
            _webEnvirontment = webEnvirontment;
            _roleManager = roleManager;
            _config = config;
        }

        public async Task SentEmail(string subject, string htmlBody, string status, string from, bool show, string toAddressTitle, string toAddress)
        {
            //send Email Here
            string FromAddress = _config["EmailSettings:SenderEmail"];
            string FromAdressTitle = _config["EmailSettings:SenderName"];
            string ToAddress = "";
            string ToAddressTitle = "";

            var bodyBuilder = new BodyBuilder();
            var mimeMessage = new MimeMessage();
            //show = false;
            ToAddress = "";
            ToAddressTitle = "";

            string Subject = subject;            
            bodyBuilder.HtmlBody = htmlBody;
            mimeMessage.From.Add(new MailboxAddress(FromAdressTitle, FromAddress));
            mimeMessage.Subject = Subject;
            mimeMessage.Body = bodyBuilder.ToMessageBody();
            ToAddressTitle = toAddressTitle;
            ToAddress = toAddress;
            mimeMessage.To.Add(new MailboxAddress(toAddressTitle, toAddress));

            //Check configuration
            var serverAddress = _config["EmailSettings:SmtpServer"];
            var emailPort = _config["EmailSettings:Port"];
            var emailUsername = _config["EmailSettings:Username"];
            var emailPass = _config["EmailSettings:Password"];

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
                    throw ex;
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

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel forgotPasswordModel)
        {
            if (!ModelState.IsValid)
                return View(forgotPasswordModel);

            var user = await _userManager.FindByEmailAsync(forgotPasswordModel.Email);

            if (user == null)
                return RedirectToAction(nameof(ForgotPasswordConfirmation));

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callback = Url.Action(nameof(ResetPassword), "Account", new { token, email = user.Email }, Request.Scheme);

            var htmlBody = "Reset Password for " + user.Email + "<br/>";
            htmlBody += "<br/>";
            htmlBody += "Here link to reset password : " + callback + "<br/>";
            htmlBody += "<br/>";
            htmlBody += "<center><small><b><i>This email is generated automatically by system.<br/>Please do not reply to this email.</i></b></small></center>";
            
            string from = "Appointment Clinic";
            string subject = "Reset Password";
            string status = "Success";
            string toTitle = user.Email;
            string toEmail = user.Email;

            await SentEmail(subject, htmlBody, status, from, true, toTitle, toEmail);
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPasswordPost(ResetPasswordViewModel resetPasswordModel)
        {
            if (!ModelState.IsValid)
                return View(resetPasswordModel);
            var user = await _userManager.FindByEmailAsync(resetPasswordModel.Email);
            if (user == null)
                RedirectToAction(nameof(ResetPasswordConfirmation));
            var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPasswordModel.Token, resetPasswordModel.Password);
            if (!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }
                return View();
            }
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }
        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        public async Task<IActionResult> Index(string sortOrder, string search, int? page)
        {
            ViewData["UserSetupParent"] = parent;
            ViewData["ListUserActive"] = active;

            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentFilter"] = search;

            ViewData["UserId"] = String.IsNullOrEmpty(sortOrder) ? "uid_d" : "";
            ViewData["Username"] = sortOrder == "usr_a" ? "usr_d" : "usr_a";
            ViewData["Email"] = sortOrder == "eml_a" ? "eml_d" : "eml_a";
            ViewData["DateCreated"] = sortOrder == "dtc_a" ? "dtc_d" : "dtc_a";
            ViewData["UserCreated"] = sortOrder == "usc_a" ? "usc_d" : "usc_a";
            ViewData["DateModified"] = sortOrder == "dtm_a" ? "dtm_d" : "dtm_a";
            ViewData["UserModified"] = sortOrder == "usm_a" ? "usm_d" : "usm_a";

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            int totalCount = 0;

            IList<RegisterViewModel> items = new List<RegisterViewModel>();

            var listUser = _userManager.Users.ToList();

            if (!String.IsNullOrEmpty(search))
            {
                listUser = listUser.Where(s => s.UserName.Contains(search)).ToList();
            }

            var sortedItems = listUser.ToList().OrderBy(i => i.UserName);

            switch (sortOrder)
            {
                case "uid_d":
                    sortedItems = sortedItems.OrderByDescending(i => i.Id);
                    break;
                case "usr_a":
                    sortedItems = sortedItems.OrderBy(i => i.UserName);
                    break;
                case "usr_d":
                    sortedItems = sortedItems.OrderByDescending(i => i.UserName);
                    break;
                case "eml_a":
                    sortedItems = sortedItems.OrderBy(i => i.Email);
                    break;
                case "eml_d":
                    sortedItems = sortedItems.OrderByDescending(i => i.Email);
                    break;
                default:
                    sortedItems = sortedItems.OrderBy(i => i.Id);
                    break;
            }

            totalCount = listUser.Count();

            foreach (var itemusr in sortedItems.ToPagedList(pageNumber, pageSize))
            {
                RegisterViewModel item = new RegisterViewModel();
                item.UserId = itemusr.Id;
                item.UserName = itemusr.UserName;
                item.Email = itemusr.Email;
                item.Name = itemusr.Name;
                string roleNameList = string.Empty;
                var roleList = await _userManager.GetRolesAsync(itemusr);

                foreach (var role in roleList)
                {
                    roleNameList = roleNameList + role + ",";
                }

                item.RoleName = roleNameList;

                items.Add(item);
            }

            IPagedList<RegisterViewModel> pagedListData = new StaticPagedList<RegisterViewModel>(items, pageNumber, pageSize, totalCount);
            return View("Index", pagedListData);
        }
        public IActionResult Register()
        {
            ViewData["UserSetupParent"] = parent;
            ViewData["ListUserActive"] = active;

            var genderList = new List<SelectListItem>()
            {
                (new SelectListItem{ Text = "Male", Value = "M"}),
                (new SelectListItem{ Text = "Female", Value = "F"})
            };

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel rgvm)
        {
            ViewData["UserSetupParent"] = parent;
            ViewData["ListUserActive"] = active;

            using (var transSql = _context.Database.BeginTransaction())
            {
                var user = new ApplicationUser
                {
                    UserName = rgvm.Email,
                    Email = rgvm.Email,
                    Name = rgvm.Name,
                    UserCreated = User.Identity.Name
                };

                if (ModelState.IsValid)
                {
                    var result = await _userManager.CreateAsync(user, rgvm.Password);

                    transSql.Commit();

                    if (result.Succeeded)
                    {
                        //ViewData["RegisterName"] = rgvm.Name;
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("UserName", error.Description);
                        }
                    }
                }

                return View(rgvm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(string userName)
        {
            ViewData["UserSetupParent"] = parent;
            ViewData["ListUserActive"] = active;

            if (userName == null)
            {
                return NotFound();
            }

            var getUser = await _context.ApplicationUser.FirstOrDefaultAsync(m => m.UserName == userName);

            if (getUser == null)
            {
                return NotFound();
            }

            return View(getUser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string userName)
        {
            ViewData["UserSetupParent"] = parent;
            ViewData["ListUserActive"] = active;

            if (userName == null)
            {
                return NotFound();
            }

            var getUser = await _context.ApplicationUser.FirstOrDefaultAsync(m => m.UserName == userName);

            if (getUser == null)
            {
                return NotFound();
            }


            RegisterViewModel rgvm = new RegisterViewModel();
            rgvm.Name = getUser.Name;
            rgvm.Email = getUser.Email;

            return View(rgvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string userName, RegisterViewModel rvm)
        {
            ViewData["UserSetupParent"] = parent;
            ViewData["ListUserActive"] = active;

            if (userName == null)
            {
                return NotFound();
            }

            var getUser = await _userManager.FindByEmailAsync(userName);

            getUser.Name = rvm.Name;
            getUser.Email = rvm.Email;

            var result = await _userManager.UpdateAsync(getUser);

            if (result.Succeeded)
            {
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Account");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            var genderList = new List<SelectListItem>()
                {
                    (new SelectListItem{ Text = "Male", Value = "M"}),
                    (new SelectListItem{ Text = "Female", Value = "F"})
                };

            ViewData["MyUser"] = getUser;

            return View(rvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string userName)
        {
            ViewData["UserSetupParent"] = parent;
            ViewData["ListUserActive"] = active;

            if (userName == null)
            {
                return NotFound();
            }

            var getUser = await _context.ApplicationUser.FirstOrDefaultAsync(m => m.UserName == userName);

            if (getUser == null)
            {
                return NotFound();
            }

            return View(getUser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userName, RegisterViewModel rvm)
        {
            ViewData["UserSetupParent"] = parent;
            ViewData["ListUserActive"] = active;

            if (userName == null)
            {
                return NotFound();
            }

            var getUser = await _userManager.FindByEmailAsync(rvm.Email);

            var result = await _userManager.DeleteAsync(getUser);

            if (result.Succeeded)
            {
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Account");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(rvm);
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity.Name == null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel lgn)
        {
            if (ModelState.IsValid)
            {
                var result = await _singInManager.PasswordSignInAsync(lgn.Email, lgn.Password, lgn.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                return View(lgn);
            }

            ModelState.AddModelError("", "Login Failed");
            return View(lgn);
        }


        [AllowAnonymous]
        public async Task<IActionResult> LogOff()
        {
            try
            {
                await _singInManager.SignOutAsync();
            }
            catch
            {

            }

            return RedirectToAction("Login", "Account");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByNameAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var cp = new ChangePasswordViewModel()
            {
                Id = user.Id
            };

            return View(cp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string id, ChangePasswordViewModel cp)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                var result = await _userManager.ChangePasswordAsync(user, cp.OldPassword, cp.NewPassword);

                if (result.Succeeded)
                {
                    await _singInManager.SignOutAsync();
                    return RedirectToAction("SuccessfulChangePassword", "Account");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                _context.Update(user);
                await _context.SaveChangesAsync();
            }
            return View(cp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPasswordAdmin(string userName)
        {
            if (userName == null)
            {
                return NotFound();
            }

            ViewData["UserName"] = userName;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPasswordAdminPost(RegisterViewModel vm)
        {
            var userPassReset = await _userManager.FindByEmailAsync(vm.Email);

            if (userPassReset != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(userPassReset);

                var result = await _userManager.ResetPasswordAsync(userPassReset, token, vm.Password);

                if (result.Succeeded)
                {
                    var email = vm.Email;
                    return RedirectToAction("ResetPasswordSuccessAdmin", "Account", email);
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View("ResetPasswordAdmin", vm);
                }
            }

            return View("ResetPasswordAdmin", vm);
        }

        public IActionResult ResetPasswordSuccessAdmin(string email)
        {
            ViewData["Email"] = email;
            return View();
        }

        public IActionResult IndexRoles(string sortOrder, string search, int? page)
        {
            ViewData["UserSetupParent"] = parent;
            ViewData["ListRolesActive"] = active;

            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentFilter"] = search;

            ViewData["RoleName"] = String.IsNullOrEmpty(sortOrder) ? "rlnm_d" : "";

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            int totalCount = 0;

            IList<RoleViewModel> items = new List<RoleViewModel>();
            var listRole = from role in _context.Roles
                           orderby role.Name ascending
                           select new
                           {
                               role.Id,
                               role.Name,
                               role.NormalizedName
                           };

            if (!String.IsNullOrEmpty(search))
            {
                listRole = listRole.Where(s => s.Name.Contains(search));
            }

            var sortedItems = listRole;

            switch (sortOrder)
            {
                case "rlnm_d":
                    sortedItems = sortedItems.OrderByDescending(i => i.Name);
                    break;
                default:
                    sortedItems = sortedItems.OrderBy(i => i.Name);
                    break;
            }

            totalCount = listRole.Count();

            foreach (var itemRole in sortedItems.ToPagedList(pageNumber, pageSize))
            {
                RoleViewModel item = new RoleViewModel();
                item.RoleId = itemRole.Id;
                item.RoleName = itemRole.Name;

                items.Add(item);
            }

            IPagedList<RoleViewModel> pagedListData = new StaticPagedList<RoleViewModel>(items, pageNumber, pageSize, totalCount);
            return View("IndexRoles", pagedListData);
        }


        [HttpGet]
        public IActionResult CreateRole()
        {
            ViewData["UserSetupParent"] = parent;
            ViewData["ListRolesActive"] = active;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(RoleViewModel rl)
        {
            ViewData["UserSetupParent"] = parent;
            ViewData["ListRolesActive"] = active;

            if (rl.RoleName == null)
            {
                ModelState.AddModelError("Name", "Cannot be Empty/Null Value");
                return View(rl);
            }

            if (ModelState.IsValid)
            {
                var role = new IdentityRole { Name = rl.RoleName };
                await _roleManager.CreateAsync(role);
                return RedirectToAction("IndexRoles");
            }

            return View("Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoles(string id)
        {
            ViewData["UserSetupParent"] = parent;
            ViewData["ListRolesActive"] = active;

            var roles = _context.Roles.Where(i => i.Id == id);
            RoleViewModel vm = new RoleViewModel();

            if (roles.Count() > 0)
            {
                var getRoles = roles.Single();
                vm.RoleId = getRoles.Id;
                vm.RoleName = getRoles.Name;

                return View(vm);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostEditRoles(string id, RoleViewModel vm)
        {
            ViewData["UserSetupParent"] = parent;
            ViewData["ListRolesActive"] = active;

            if (id != vm.RoleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    IdentityRole thisRole = await _roleManager.FindByIdAsync(vm.RoleId);
                    thisRole.Name = vm.RoleName;
                    await _roleManager.UpdateAsync(thisRole);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DbExtensionExists(vm.RoleId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(IndexRoles));
            }
            return View(vm);
        }

        private bool DbExtensionExists(string roleId)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoles(string id)
        {
            ViewData["UserSetupParent"] = parent;
            ViewData["ListRolesActive"] = active;

            var roles = _context.Roles.Where(i => i.Id == id);
            RoleViewModel vm = new RoleViewModel();

            if (roles.Count() > 0)
            {
                var getRoles = roles.Single();
                vm.RoleId = getRoles.Id;
                vm.RoleName = getRoles.Name;

                return View(vm);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostDeleteRoles(string id, RoleViewModel vm)
        {
            ViewData["UserSetupParent"] = parent;
            ViewData["ListRolesActive"] = active;

            var delRole = await _context.Roles.FindAsync(id);
            _context.Roles.Remove(delRole);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(IndexRoles));
        }

        private async Task<ApplicationUser> GetUserById(string id) =>
     await _userManager.FindByIdAsync(id);

        private SelectList GetAllRoles() =>

            new SelectList(_roleManager.Roles.OrderBy(r => r.Name));
        [HttpGet]
        public async Task<IActionResult> AddRole(string userId)
        {
            ViewData["UserSetupParent"] = parent;
            ViewData["ListUserActive"] = active;

            var user = await GetUserById(userId);

            var getUserRole = await _userManager.GetRolesAsync(user);

            List<RoleViewModel> items = new List<RoleViewModel>();

            var roleList = _context.UserRoles.ToList().Where(c => c.UserId == user.Id);

            foreach (var role in roleList)
            {
                var item = new RoleViewModel();

                var roleName = _roleManager.Roles.Where(r => r.Id == role.RoleId).Single();
                item.RoleName = roleName.Name;
                item.RoleId = roleName.Id;
                item.UserId = user.Id;
                items.Add(item);
            }

            var vm = new AddRoleViewModel
            {
                Roles = GetAllRoles(),
                UserId = userId,
                Name = user.Name
            };

            vm.RoleListUser = items;

            return View(vm);
        }

        [HttpPost]
        //[AutoValidateAntiforgeryToken]
        public async Task<IActionResult> AddRole(AddRoleViewModel vm)
        {
            ViewData["UserSetupParent"] = parent;
            ViewData["ListUserActive"] = active;

            if (ModelState.IsValid)
            {
                var user = await GetUserById(vm.UserId);
                var result = await _userManager.AddToRoleAsync(user, vm.NewRole);

                if (result.Succeeded)
                {
                    return RedirectToAction("AddRole", "Account", new { userId = user.Id });
                    //return View("AddRole", vm);
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ViewData["ErrorAddRole"] = error.Description;
                        return RedirectToAction("AddRole", "Account", new { userId = user.Id });
                    }
                }
            }

            vm.Roles = GetAllRoles();
            return View("AddRole", vm);
        }

        public async Task<IActionResult> RemoveUserRole(string userId, string roleId)
        {
            ViewData["UserSetupParent"] = parent;
            ViewData["ListUserActive"] = active;

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    var existingRoleId = _roleManager.Roles.Where(r => r.Id == roleId).Single();
                    var roleResult = await _userManager.RemoveFromRoleAsync(user, existingRoleId.Name);

                    if (roleResult.Succeeded)
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("AddRole", "Account", new { userId = user.Id });
                    }
                }
            }

            return View();
        }

        private bool DbExtensionExistsRoles(string id)
        {
            return _roleManager.Roles.Any(e => e.Id == id);
        }
    }
}
