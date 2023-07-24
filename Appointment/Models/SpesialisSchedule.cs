using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Appointment.Models
{
    public class SpesialisSchedule
    {
        [Key]
        public int IdSpesialisSchedule { get; set; }
        [ForeignKey("Spesialis")]
        public int IdSpesialis { get; set; }
        public string UserId { get; set; }
        public string ScheduleDay { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public string UserCreated { get; set; }
        public DateTime DateCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime DateModified { get; set; }

        public Spesialis Spesialis { get; set; }
    }
}
