using Microsoft.AspNetCore.Identity;

namespace Appointment.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string UserCreated { get; set; }
        public DateTime BirthDate { get; set; }
        public string Address { get; set; }
        public string IdNumber { get; set; }
    }
}
