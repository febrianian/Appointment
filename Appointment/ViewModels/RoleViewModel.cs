using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Appointment.ViewModels
{
    public class RoleViewModel
    {
        public List<IdentityRole> Roles { get; set; }
        public string RoleId { get; set; }
        public string UserId { get; set; }
        public string RoleName { get; set; }
    }
}
