using Microsoft.AspNetCore.Identity;

namespace Appointment.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}
