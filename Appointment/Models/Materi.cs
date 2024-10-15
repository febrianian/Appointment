using System.ComponentModel.DataAnnotations;

namespace Appointment.Models
{
    public class Materi
    {
        [Key]
        public int IdMateri { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
