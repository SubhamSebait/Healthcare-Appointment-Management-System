namespace HealthcareApp.API.DTOs
{
    // ── Auth ─────────────────────────────────────────────────────────────
    public class RegisterDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public int? ProfileId { get; set; }  // PatientId or DoctorId
    }

    // ── Doctor ───────────────────────────────────────────────────────────
    public class DoctorListDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string SpecializationName { get; set; } = string.Empty;
        public int SpecializationId { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? Bio { get; set; }
    }

    public class CreateDoctorDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int SpecializationId { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? Bio { get; set; }
    }

    public class UpdateDoctorDto
    {
        public string FullName { get; set; } = string.Empty;
        public int SpecializationId { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? Bio { get; set; }
    }

    // ── Specialization ───────────────────────────────────────────────────
    public class SpecializationDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class CreateSpecializationDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    // ── Availability ─────────────────────────────────────────────────────
    public class AvailabilityDto
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public DateTime Date { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public bool IsBooked { get; set; }
    }

    public class CreateAvailabilityDto
    {
        public DateTime Date { get; set; }
        public string StartTime { get; set; } = string.Empty;  // "HH:mm"
        public string EndTime { get; set; } = string.Empty;    // "HH:mm"
    }

    // ── Appointment ──────────────────────────────────────────────────────
    public class BookAppointmentDto
    {
        public int AvailabilityId { get; set; }
        public string? Notes { get; set; }
    }

    public class AppointmentDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string SpecializationName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? DoctorNotes { get; set; }
        public DateTime BookedAt { get; set; }
    }

    public class UpdateStatusDto
    {
        public string? DoctorNotes { get; set; }
    }
}
