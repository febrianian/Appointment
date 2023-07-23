using Appointment.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Appointment.ViewModels
{
    public class AddRoleViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }

        public string RoleId { get; set; }

        [Display(Name = "Role Name")]
        public string RoleName { get; set; }
        public string NewRole { get; set; }
        public SelectList Roles { get; set; }
        public string Name { get; set; }
        public List<RoleViewModel> RoleListUser { get; set; }
    }
}
