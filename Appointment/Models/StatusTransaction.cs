using System.ComponentModel.DataAnnotations;

namespace Appointment.Models
{
    public class StatusTransaction
    {
        [Key]
        public string IdStatus { get; set; }
        public string StatusName { get; set; }
        public string Status { get; set; }
        public string UserCreated { get; set; }
        public DateTime DateCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime DateModified { get; set; }
    }
}
