using HealthcareApp.API.Data;
using HealthcareApp.API.DTOs;
using HealthcareApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthcareApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(AppDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger  = logger;
        }

        // ── Doctors ──────────────────────────────────────────────────────

        /// <summary>Get all doctors (Admin)</summary>
        [HttpGet("doctors")]
        public async Task<IActionResult> GetDoctors()
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialization)
                .Select(d => new DoctorListDto
                {
                    Id                 = d.Id,
                    FullName           = d.User.FullName,
                    SpecializationName = d.Specialization.Name,
                    SpecializationId   = d.SpecializationId,
                    Phone              = d.Phone,
                    Bio                = d.Bio
                })
                .ToListAsync();
            return Ok(doctors);
        }

        /// <summary>Add a new doctor (creates User + Doctor records)</summary>
        [HttpPost("doctors")]
        public async Task<IActionResult> AddDoctor([FromBody] CreateDoctorDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { message = "Email already in use." });

            var spec = await _context.Specializations.FindAsync(dto.SpecializationId);
            if (spec == null) return BadRequest(new { message = "Specialization not found." });

            var user = new User
            {
                FullName     = dto.FullName,
                Email        = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role         = "Doctor"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var doctor = new Doctor
            {
                UserId           = user.Id,
                SpecializationId = dto.SpecializationId,
                Phone            = dto.Phone,
                Bio              = dto.Bio
            };
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin added doctor: {Name} ({Email})", dto.FullName, dto.Email);
            return Ok(new DoctorListDto
            {
                Id                 = doctor.Id,
                FullName           = user.FullName,
                SpecializationName = spec.Name,
                SpecializationId   = spec.Id,
                Phone              = doctor.Phone,
                Bio                = doctor.Bio
            });
        }

        /// <summary>Update doctor info</summary>
        [HttpPut("doctors/{id}")]
        public async Task<IActionResult> UpdateDoctor(int id, [FromBody] UpdateDoctorDto dto)
        {
            var doctor = await _context.Doctors.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == id);
            if (doctor == null) return NotFound(new { message = "Doctor not found." });

            var spec = await _context.Specializations.FindAsync(dto.SpecializationId);
            if (spec == null) return BadRequest(new { message = "Specialization not found." });

            doctor.User.FullName  = dto.FullName;
            doctor.SpecializationId = dto.SpecializationId;
            doctor.Phone          = dto.Phone;
            doctor.Bio            = dto.Bio;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin updated doctor {DoctorId}", id);
            return Ok(new { message = "Doctor updated." });
        }

        /// <summary>Delete a doctor (and their user account)</summary>
        [HttpDelete("doctors/{id}")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var doctor = await _context.Doctors.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == id);
            if (doctor == null) return NotFound(new { message = "Doctor not found." });

            // Check for active appointments
            bool hasActive = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == id &&
                (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed));

            if (hasActive)
                return BadRequest(new { message = "Cannot delete doctor with active appointments. Resolve them first." });

            _context.Users.Remove(doctor.User); // cascade deletes doctor record
            await _context.SaveChangesAsync();
            _logger.LogInformation("Admin deleted doctor {DoctorId}", id);
            return Ok(new { message = "Doctor deleted." });
        }

        // ── Patients ─────────────────────────────────────────────────────

        /// <summary>Get all patients</summary>
        [HttpGet("patients")]
        public async Task<IActionResult> GetPatients()
        {
            var patients = await _context.Patients
                .Include(p => p.User)
                .Select(p => new
                {
                    p.Id,
                    FullName    = p.User.FullName,
                    Email       = p.User.Email,
                    p.Phone,
                    p.DateOfBirth
                })
                .ToListAsync();
            return Ok(patients);
        }
    }
}
