using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Appointment.Models
{
    public class Transaction
    {
        [Key]
        public int IdTransaction { get; set; }
        public string PatientName { get; set; }
        public string DoctorId { get; set; }
        [ForeignKey("StatusTransaction")]
        public string IdStatus { get; set; }
        public string Status { get; set; }
        public string UserCreated { get; set; }
        public DateTime DateCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime DateModified { get; set; }

        public StatusTransaction StatusTransaction { get; set; }
        
    }
}
