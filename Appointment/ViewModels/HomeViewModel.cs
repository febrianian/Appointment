using Appointment.Models;

namespace Appointment.ViewModels
{
    public class HomeViewModel
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public List<AppointmentClinicViewModel> ListTransactionPatient { get; set; }
    }
}
