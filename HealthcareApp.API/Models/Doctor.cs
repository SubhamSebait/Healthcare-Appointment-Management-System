namespace HealthcareApp.API.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SpecializationId { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? ProfileImage { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public Specialization Specialization { get; set; } = null!;
        public ICollection<Availability> Availabilities { get; set; } = new List<Availability>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
