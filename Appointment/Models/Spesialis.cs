namespace Appointment.Models
{
    public class Spesialis
    {
        public int Id { get; set; }
        public string SpesialisName { get; set; }
        public string Status { get; set; }
        public string UserCreated { get; set; }
        public DateTime DateCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime DateModified { get; set; }
        public string ImagesPath { get; set; }
        public string Description { get; set; }
    }
}
