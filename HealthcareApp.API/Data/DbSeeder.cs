using HealthcareApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthcareApp.API.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Only seed if database is empty
            if (await context.Users.AnyAsync()) return;

            // ── Specializations ──────────────────────────────────────────
            var specializations = new List<Specialization>
            {
                new Specialization { Name = "General Physician",  Description = "Primary care and general health management" },
                new Specialization { Name = "Dentist",            Description = "Oral health and dental care" },
                new Specialization { Name = "Dermatologist",      Description = "Skin, hair, and nail conditions" },
                new Specialization { Name = "Orthopedist",        Description = "Bone, joint, and muscle conditions" }
            };
            context.Specializations.AddRange(specializations);
            await context.SaveChangesAsync();

            // ── Admin User ───────────────────────────────────────────────
            var adminUser = new User
            {
                FullName = "Admin User",
                Email = "admin@healthcare.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = "Admin"
            };
            context.Users.Add(adminUser);
            await context.SaveChangesAsync();

            // ── Doctor Users ─────────────────────────────────────────────
            var doctorData = new[]
            {
                ("Dr. Arjun Sharma",   "arjun@healthcare.com",   "Doctor@123", specializations[0].Id, "9876543210", "MBBS, MD - 10 years of experience in general medicine."),
                ("Dr. Priya Mehta",    "priya@healthcare.com",   "Doctor@123", specializations[1].Id, "9876543211", "BDS, MDS - Expert in cosmetic and restorative dentistry."),
                ("Dr. Rohan Verma",    "rohan@healthcare.com",   "Doctor@123", specializations[2].Id, "9876543212", "MBBS, MD Dermatology - Specializes in acne, eczema, and skin care."),
                ("Dr. Sunita Patel",   "sunita@healthcare.com",  "Doctor@123", specializations[3].Id, "9876543213", "MBBS, MS Ortho - Expert in joint replacement and sports injuries."),
                ("Dr. Kavita Nair",    "kavita@healthcare.com",  "Doctor@123", specializations[0].Id, "9876543214", "MBBS - Family physician with 8 years experience.")
            };

            var doctors = new List<Doctor>();
            foreach (var (name, email, pass, specId, phone, bio) in doctorData)
            {
                var user = new User { FullName = name, Email = email, PasswordHash = BCrypt.Net.BCrypt.HashPassword(pass), Role = "Doctor" };
                context.Users.Add(user);
                await context.SaveChangesAsync();
                var doctor = new Doctor { UserId = user.Id, SpecializationId = specId, Phone = phone, Bio = bio };
                context.Doctors.Add(doctor);
                doctors.Add(doctor);
            }
            await context.SaveChangesAsync();

            // ── Patient Users ────────────────────────────────────────────
            var patientData = new[]
            {
                ("Rahul Singh",  "rahul@patient.com",  "Patient@123", "9123456789", new DateTime(1995, 6, 15)),
                ("Neha Gupta",   "neha@patient.com",   "Patient@123", "9123456790", new DateTime(1998, 3, 22)),
                ("Amit Kumar",   "amit@patient.com",   "Patient@123", "9123456791", new DateTime(1990, 11, 5))
            };

            var patients = new List<Patient>();
            foreach (var (name, email, pass, phone, dob) in patientData)
            {
                var user = new User { FullName = name, Email = email, PasswordHash = BCrypt.Net.BCrypt.HashPassword(pass), Role = "Patient" };
                context.Users.Add(user);
                await context.SaveChangesAsync();
                var patient = new Patient { UserId = user.Id, Phone = phone, DateOfBirth = dob };
                context.Patients.Add(patient);
                patients.Add(patient);
            }
            await context.SaveChangesAsync();

            // ── Availability Slots ───────────────────────────────────────
            var today = DateTime.Today;
            var slots = new List<Availability>();

            // Dr. Arjun Sharma - doctors[0]
            for (int day = 1; day <= 5; day++)
            {
                var slotDate = today.AddDays(day);
                slots.Add(new Availability { DoctorId = doctors[0].Id, Date = slotDate, StartTime = new TimeSpan(9, 0, 0),  EndTime = new TimeSpan(9, 30, 0) });
                slots.Add(new Availability { DoctorId = doctors[0].Id, Date = slotDate, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(10, 30, 0) });
                slots.Add(new Availability { DoctorId = doctors[0].Id, Date = slotDate, StartTime = new TimeSpan(11, 0, 0), EndTime = new TimeSpan(11, 30, 0) });
            }

            // Dr. Priya Mehta - doctors[1]
            for (int day = 1; day <= 5; day++)
            {
                var slotDate = today.AddDays(day);
                slots.Add(new Availability { DoctorId = doctors[1].Id, Date = slotDate, StartTime = new TimeSpan(14, 0, 0), EndTime = new TimeSpan(14, 30, 0) });
                slots.Add(new Availability { DoctorId = doctors[1].Id, Date = slotDate, StartTime = new TimeSpan(15, 0, 0), EndTime = new TimeSpan(15, 30, 0) });
            }

            // Dr. Rohan Verma - doctors[2]
            for (int day = 2; day <= 6; day++)
            {
                var slotDate = today.AddDays(day);
                slots.Add(new Availability { DoctorId = doctors[2].Id, Date = slotDate, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(10, 30, 0) });
                slots.Add(new Availability { DoctorId = doctors[2].Id, Date = slotDate, StartTime = new TimeSpan(16, 0, 0), EndTime = new TimeSpan(16, 30, 0) });
            }

            // Dr. Sunita Patel - doctors[3]
            slots.Add(new Availability { DoctorId = doctors[3].Id, Date = today.AddDays(1), StartTime = new TimeSpan(9, 0, 0),  EndTime = new TimeSpan(9, 30, 0) });
            slots.Add(new Availability { DoctorId = doctors[3].Id, Date = today.AddDays(2), StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(10, 30, 0) });
            slots.Add(new Availability { DoctorId = doctors[3].Id, Date = today.AddDays(3), StartTime = new TimeSpan(11, 0, 0), EndTime = new TimeSpan(11, 30, 0) });

            context.Availabilities.AddRange(slots);
            await context.SaveChangesAsync();

            // ── Sample Appointments ──────────────────────────────────────
            // Mark the first slot of Dr. Arjun as booked
            var bookedSlot = slots[0];
            bookedSlot.IsBooked = true;
            var appt1 = new Appointment
            {
                PatientId      = patients[0].Id,
                DoctorId       = doctors[0].Id,
                AvailabilityId = bookedSlot.Id,
                Status         = AppointmentStatus.Confirmed,
                Notes          = "Regular checkup",
                BookedAt       = DateTime.UtcNow.AddDays(-1)
            };

            var bookedSlot2 = slots[3];
            bookedSlot2.IsBooked = true;
            var appt2 = new Appointment
            {
                PatientId      = patients[1].Id,
                DoctorId       = doctors[0].Id,
                AvailabilityId = bookedSlot2.Id,
                Status         = AppointmentStatus.Pending,
                Notes          = "Fever and cold",
                BookedAt       = DateTime.UtcNow
            };

            context.Appointments.AddRange(appt1, appt2);
            await context.SaveChangesAsync();
        }
    }
}
