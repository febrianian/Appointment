namespace Appointment.ViewModels
{
    public class AppointmentClinicViewModel
    {
        public int IdAppointment { get; set; }
        public int IdSpesialis { get; set; }
        public string Spesialis { get; set; }
        public string DoctorNote { get; set; }
        public string UserIdPatient { get; set; }
        public string DoctorName { get; set; }
        public string UserIdDoctor { get; set; }
        public string PatientName { get; set; }
        public string Day { get; set; }
        public string Age { get; set; }
        public DateTime DateAppointment { get; set; }
        public string TimeAppointment { get; set; }
        public string ReasonOfSick { get; set; }
        public string HistoryOfSick { get; set; }
        public string IdStatus { get; set; }
        public string StatusName { get; set; }
        public string Status { get; set; }
        public string UserCreated { get; set; }
        public DateTime DateCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime DateModified { get; set; }
    }
}
