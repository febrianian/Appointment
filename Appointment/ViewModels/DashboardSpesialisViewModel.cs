namespace Appointment.ViewModels
{
    public class DashboardSpesialisViewModel
    {
        public int IdSpesialis { get; set; }
        public string UserId { get; set; }
        public List<SpesialisScheduleViewModel> ListSpesialis { get; set; }
        public List<SpesialisScheduleViewModel> ListSpesialisHours { get; set; }
    }
}
