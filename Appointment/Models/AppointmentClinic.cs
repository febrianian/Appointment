using System.ComponentModel.DataAnnotations;

namespace Appointment.Models
{
    public class AppointmentClinic
    {                
        [Key]
        public int IdAppointment { get; set; }
        public int IdSpesialis { get; set; }
        public string UserIdPatient { get; set; }
        public string UserIdDoctor { get; set; }
        public string Day { get; set; }
        public string Age { get; set; }
        public DateTime DateAppointment { get; set; }
        public string TimeAppointment { get; set; }
        public string ReasonOfSick { get; set; }
        public string HistoryOfSick { get; set; }       
        public string IdStatus { get; set; }
        public string Status { get; set; }
        public string UserCreated { get; set; }
        public DateTime DateCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime DateModified { get; set; }
        public string TicketNo { get; set; }
        public string DoctorNote { get; set; }
        public string QRCode { get; set; }
        public string QRCodePath { get; set; }
    }
}
