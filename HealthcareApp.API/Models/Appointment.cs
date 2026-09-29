namespace HealthcareApp.API.Models
{
    public enum AppointmentStatus
    {
        Pending,
        Confirmed,
        Rejected,
        Cancelled,
        Completed
    }

    public class Appointment
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public int AvailabilityId { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
        public string? Notes { get; set; }  // Patient notes / reason
        public string? DoctorNotes { get; set; }  // Doctor remarks
        public DateTime BookedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Patient Patient { get; set; } = null!;
        public Doctor Doctor { get; set; } = null!;
        public Availability Availability { get; set; } = null!;
    }
}
