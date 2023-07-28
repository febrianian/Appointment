using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Appointment.Models
{
    public class AppointmentContext : IdentityDbContext<ApplicationUser>
    {
        public AppointmentContext(DbContextOptions<AppointmentContext> options) : base(options) { }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        public DbSet<AppointmentClinic> AppointmentClinic { get; set; }
        public DbSet<Spesialis> Spesialis { get; set; }
        public DbSet<SpesialisSchedule> SpesialisSchedule { get; set; }
        public DbSet<StatusTransaction> StatusTransaction { get; set; }
    }
}
