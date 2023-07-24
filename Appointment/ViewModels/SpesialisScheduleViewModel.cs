using Appointment.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Appointment.ViewModels
{
    public class SpesialisScheduleViewModel
    {
        public int IdSpesialisSchedule { get; set; }
        public int IdSpesialis { get; set; }
        public string SpesialisName { get; set; }
        public string UserId { get; set; }
        public string ScheduleDay { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Status { get; set; }
        public string UserCreated { get; set; }
        public DateTime DateCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime DateModified { get; set; }
    }
}
