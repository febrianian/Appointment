using System.ComponentModel.DataAnnotations;

namespace Appointment.ViewModels
{
    public class SpesialisViewModel
    {
        public int Id { get; set; }
        [Required]
        public string SpesialisName { get; set; }
        public IFormFile files { get; set; }
        public string ImagesPath { get; set; }
        public string Status { get; set; }
        public string UserCreated { get; set; }
        public DateTime DateCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime DateModified { get; set; }
        public string Description { get; set; }
    }
}
