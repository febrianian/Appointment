using Appointment.Models;
using Appointment.ViewModels;
using Appointment.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Appointment.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly AppointmentContext _db;
        public AppointmentService(AppointmentContext db)
        {
            _db = db;
        }

        public async Task<int> AddUpdate(AppointmentVM model)
        {
            var startDate = DateTime.Parse(model.StartDate);
            var endDate = DateTime.Parse(model.StartDate).AddMinutes(Convert.ToDouble(model.Duration));
            var patient = _db.Users.FirstOrDefault(u => u.Id == model.PatientId);
            var doctor = _db.Users.FirstOrDefault(u => u.Id == model.DoctorId);
            if (model != null && model.IdAppointment > 0)
            {
                //update
                var appointmentUpdate = _db.AppointmentClinic.FirstOrDefault(x => x.IdAppointment == model.IdAppointment);
                appointmentUpdate.Title = model.Title;
                appointmentUpdate.Description = model.Description;
                appointmentUpdate.StartDate = startDate;
                appointmentUpdate.EndDate = endDate;
                appointmentUpdate.Duration = model.Duration;
                appointmentUpdate.DoctorId = model.DoctorId;
                appointmentUpdate.PatientId = model.PatientId;
                appointmentUpdate.IsDoctorApproved = false;
                appointmentUpdate.AdminId = model.AdminId;
                _db.AppointmentClinic.Update(appointmentUpdate);
                await _db.SaveChangesAsync();
                return 1;
            }
            else
            {
                //create
                AppointmentClinic appointmentNew = new AppointmentClinic()
                {
                    Title = model.Title,
                    Description = model.Description,
                    StartDate = startDate,
                    EndDate = endDate,
                    Duration = model.Duration,
                    DoctorId = model.DoctorId,
                    PatientId = model.PatientId,
                    IsDoctorApproved = false,
                    AdminId = "A"
                };
                //await _emailSender.SendEmailAsync(doctor.Email, "Appointment Created",
                //    $"Your appointment with {patient.Name} is created and in pending status");
                //await _emailSender.SendEmailAsync(patient.Email, "Appointment Created",
                //    $"Your appointment with {doctor.Name} is created and in pending status");
                _db.AppointmentClinic.Add(appointmentNew);
                await _db.SaveChangesAsync();
                return 2;
            }

        }

        public async Task<int> ConfirmEvent(int id)
        {
            var appointment = _db.AppointmentClinic.FirstOrDefault(x => x.IdAppointment == id);
            if (appointment != null)
            {
                appointment.IsDoctorApproved = true;
                return await _db.SaveChangesAsync();
            }
            return 0;
        }

        public async Task<int> Delete(int id)
        {
            var appointment = _db.AppointmentClinic.FirstOrDefault(x => x.IdAppointment == id);
            if (appointment != null)
            {
                _db.AppointmentClinic.Remove(appointment);
                return await _db.SaveChangesAsync();
            }
            return 0;
        }

        public List<AppointmentVM> DoctorsEventsById(string doctorId)
        {
            return _db.AppointmentClinic.Where(x => x.DoctorId == doctorId).ToList().Select(c => new AppointmentVM()
            {
                IdAppointment = c.IdAppointment,
                Description = c.Description,
                StartDate = c.StartDate.ToString("yyyy-MM-dd HH:mm:ss"),
                EndDate = c.EndDate.ToString("yyyy-MM-dd HH:mm:ss"),
                Title = c.Title,
                Duration = c.Duration,
                IsDoctorApproved = c.IsDoctorApproved
            }).ToList();
        }

        public AppointmentVM GetById(int id)
        {
            return _db.AppointmentClinic.Where(x => x.IdAppointment == id).ToList().Select(c => new AppointmentVM()
            {
                IdAppointment = c.IdAppointment,
                Description = c.Description,
                StartDate = c.StartDate.ToString("yyyy-MM-dd HH:mm:ss"),
                EndDate = c.EndDate.ToString("yyyy-MM-dd HH:mm:ss"),
                Title = c.Title,
                Duration = c.Duration,
                IsDoctorApproved = c.IsDoctorApproved,
                PatientId = c.PatientId,
                DoctorId = c.DoctorId,
                PatientName = _db.Users.Where(x => x.Id == c.PatientId).Select(x => x.Name).FirstOrDefault(),
                DoctorName = _db.Users.Where(x => x.Id == c.DoctorId).Select(x => x.Name).FirstOrDefault(),
            }).SingleOrDefault();
        }

        public List<DoctorVM> GetDoctorList()
        {
            var doctors = (from user in _db.Users
                           join userRoles in _db.UserRoles on user.Id equals userRoles.UserId
                           join roles in _db.Roles on userRoles.RoleId equals roles.Id
                           where roles.Name == "Doctor"
                           select new DoctorVM
                           {
                               Id = user.Id,
                               Name = user.Name
                           }
                           ).ToList();

            return doctors;
        }

        public List<PatientVM> GetPatientList()
        {
            var patients = (from user in _db.Users
                            join userRoles in _db.UserRoles on user.Id equals userRoles.UserId
                            join roles in _db.Roles.Where(x => x.Name == Helper.Patient) on userRoles.RoleId equals roles.Id
                            select new PatientVM
                            {
                                Id = user.Id,
                                Name = user.Name
                            }
                           ).ToList();

            return patients;
        }

        public List<SpesialisViewModel> GetSpesialistList()
        {
            var spesialis = (from spesial in _db.Spesialis
                             where spesial.Status == "A"
                             select new SpesialisViewModel
                             {
                                 Id = spesial.Id,
                                 SpesialisName = spesial.SpesialisName
                             }
                           ).ToList();

            return spesialis;
        }

        public List<AppointmentVM> PatientsEventsById(string patientId)
        {
            return _db.AppointmentClinic.Where(x => x.PatientId == patientId).ToList().Select(c => new AppointmentVM()
            {
                IdAppointment = c.IdAppointment,
                Description = c.Description,
                StartDate = c.StartDate.ToString("yyyy-MM-dd HH:mm:ss"),
                EndDate = c.EndDate.ToString("yyyy-MM-dd HH:mm:ss"),
                Title = c.Title,
                Duration = c.Duration,
                IsDoctorApproved = c.IsDoctorApproved
            }).ToList();
        }
    }
}
