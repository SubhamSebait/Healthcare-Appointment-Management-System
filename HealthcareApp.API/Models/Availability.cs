namespace HealthcareApp.API.Models
{
    public class Availability
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsBooked { get; set; } = false;

        // Navigation
        public Doctor Doctor { get; set; } = null!;
        public Appointment? Appointment { get; set; }
    }
}
