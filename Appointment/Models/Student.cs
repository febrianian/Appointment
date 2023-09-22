using System.ComponentModel.DataAnnotations;

namespace Appointment.Models
{
    public class Student
    {
        [Key]
        public int IdStudent { get; set; }
        public string StudentName { get; set; }
        public string PhoneNumber { get; set; }
        public double NilaiUjian { get; set; }
        public DateTime DateCreated { get; set; }

    }
}
