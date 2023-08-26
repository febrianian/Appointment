using Appointment.Models;

namespace Appointment.ViewModels
{
    public class HomeViewModel
    {
        public string UserId { get; set; }
        public int IdAppointment { get; set; }
        public string NamePatient { get; set; }
        public string NameDoctor { get; set; }
        public List<AppointmentClinicViewModel> ListTransactionPatient { get; set; }
        public List<AppointmentClinicViewModel> ListTransactionDoctor { get; set; }
    }
}
