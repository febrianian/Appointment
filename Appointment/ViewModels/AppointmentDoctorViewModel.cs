namespace Appointment.ViewModels
{
    public class AppointmentDoctorViewModel
    {
        public int IdAppointment { get; set; }
        public int IdSpesialis { get; set; }
        public string UserId { get; set; }
        public string DoctorName { get; set; }
        public string Spesialis { get; set; }
        public DateTime DateAppointment { get; set; }
        public string TimeAppointment { get; set; }
        public string ReasonOfSick { get; set; }
        public string HistoryOfSick { get; set; }

        //identity
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime BirthDate { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Age { get; set; }
        public string Day { get; set; }
    }
}
