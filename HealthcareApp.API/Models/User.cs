namespace HealthcareApp.API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Patient"; // Patient, Doctor, Admin

        // Navigation
        public Patient? Patient { get; set; }
        public Doctor? Doctor { get; set; }
    }
}
