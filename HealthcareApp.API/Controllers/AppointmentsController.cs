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
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(AppDbContext context, ILogger<AppointmentsController> logger)
        {
            _context = context;
            _logger  = logger;
        }

        // ── Helpers ──────────────────────────────────────────────────────

        private static AppointmentDto MapToDto(Appointment a) => new()
        {
            Id                 = a.Id,
            PatientId          = a.PatientId,
            PatientName        = a.Patient.User.FullName,
            DoctorId           = a.DoctorId,
            DoctorName         = a.Doctor.User.FullName,
            SpecializationName = a.Doctor.Specialization.Name,
            Date               = a.Availability.Date,
            StartTime          = a.Availability.StartTime.ToString(@"hh\:mm"),
            EndTime            = a.Availability.EndTime.ToString(@"hh\:mm"),
            Status             = a.Status.ToString(),
            Notes              = a.Notes,
            DoctorNotes        = a.DoctorNotes,
            BookedAt           = a.BookedAt
        };

        private IQueryable<Appointment> BaseQuery() => _context.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
            .Include(a => a.Availability);

        // ── Patient Actions ──────────────────────────────────────────────

        /// <summary>Book an appointment (Patient only)</summary>
        [HttpPost]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Book([FromBody] BookAppointmentDto dto)
        {
            var profileIdClaim = User.FindFirst("ProfileId")?.Value;
            if (!int.TryParse(profileIdClaim, out int patientId))
                return Unauthorized(new { message = "Patient profile not found." });

            // Check slot exists and is free
            var slot = await _context.Availabilities.Include(a => a.Doctor).FirstOrDefaultAsync(a => a.Id == dto.AvailabilityId);
            if (slot == null)          return NotFound(new { message = "Time slot not found." });
            if (slot.IsBooked)         return BadRequest(new { message = "This time slot is already booked." });
            if (slot.Date < DateTime.Today) return BadRequest(new { message = "Cannot book a past slot." });

            slot.IsBooked = true;
            var appointment = new Appointment
            {
                PatientId      = patientId,
                DoctorId       = slot.DoctorId,
                AvailabilityId = slot.Id,
                Status         = AppointmentStatus.Pending,
                Notes          = dto.Notes,
                BookedAt       = DateTime.UtcNow
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Patient {PatientId} booked appointment with Doctor {DoctorId} on {Date}",
                patientId, slot.DoctorId, slot.Date.ToShortDateString());

            // Reload with nav properties for response
            var saved = await BaseQuery().FirstAsync(a => a.Id == appointment.Id);
            return CreatedAtAction(nameof(GetMyAppointments), null, MapToDto(saved));
        }

        /// <summary>Get logged-in patient's appointments</summary>
        [HttpGet("my")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var profileIdClaim = User.FindFirst("ProfileId")?.Value;
            if (!int.TryParse(profileIdClaim, out int patientId))
                return Unauthorized();

            var appts = await BaseQuery()
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.BookedAt)
                .ToListAsync();

            return Ok(appts.Select(MapToDto));
        }

        /// <summary>Cancel appointment (Patient only, must be Pending or Confirmed)</summary>
        [HttpPatch("{id}/cancel")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Cancel(int id)
        {
            var profileIdClaim = User.FindFirst("ProfileId")?.Value;
            if (!int.TryParse(profileIdClaim, out int patientId))
                return Unauthorized();

            var appt = await _context.Appointments.Include(a => a.Availability).FirstOrDefaultAsync(a => a.Id == id);
            if (appt == null)              return NotFound(new { message = "Appointment not found." });
            if (appt.PatientId != patientId) return Forbid();

            if (appt.Status == AppointmentStatus.Completed || appt.Status == AppointmentStatus.Rejected || appt.Status == AppointmentStatus.Cancelled)
                return BadRequest(new { message = $"Cannot cancel an appointment with status '{appt.Status}'." });

            appt.Status           = AppointmentStatus.Cancelled;
            appt.Availability.IsBooked = false;  // free the slot
            await _context.SaveChangesAsync();

            _logger.LogInformation("Patient {PatientId} cancelled appointment {AppointmentId}", patientId, id);
            return Ok(new { message = "Appointment cancelled." });
        }

        // ── Doctor Actions ───────────────────────────────────────────────

        /// <summary>Get doctor's appointments</summary>
        [HttpGet("doctor")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetDoctorAppointments()
        {
            var profileIdClaim = User.FindFirst("ProfileId")?.Value;
            if (!int.TryParse(profileIdClaim, out int doctorId))
                return Unauthorized();

            var appts = await BaseQuery()
                .Where(a => a.DoctorId == doctorId)
                .OrderByDescending(a => a.Availability.Date)
                .ToListAsync();

            return Ok(appts.Select(MapToDto));
        }

        /// <summary>Confirm an appointment (Doctor only)</summary>
        [HttpPatch("{id}/confirm")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Confirm(int id, [FromBody] UpdateStatusDto? dto)
        {
            var appt = await GetDoctorAppointment(id);
            if (appt == null) return NotFound(new { message = "Appointment not found or not yours." });
            if (appt.Status != AppointmentStatus.Pending) return BadRequest(new { message = "Only Pending appointments can be confirmed." });

            appt.Status      = AppointmentStatus.Confirmed;
            appt.DoctorNotes = dto?.DoctorNotes;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Doctor confirmed appointment {AppointmentId}", id);
            return Ok(new { message = "Appointment confirmed." });
        }

        /// <summary>Reject an appointment (Doctor only)</summary>
        [HttpPatch("{id}/reject")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Reject(int id, [FromBody] UpdateStatusDto? dto)
        {
            var appt = await GetDoctorAppointment(id);
            if (appt == null) return NotFound(new { message = "Appointment not found or not yours." });
            if (appt.Status != AppointmentStatus.Pending) return BadRequest(new { message = "Only Pending appointments can be rejected." });

            appt.Status                = AppointmentStatus.Rejected;
            appt.DoctorNotes           = dto?.DoctorNotes;
            appt.Availability.IsBooked = false;  // free the slot so another patient can book
            await _context.SaveChangesAsync();

            _logger.LogInformation("Doctor rejected appointment {AppointmentId}", id);
            return Ok(new { message = "Appointment rejected." });
        }

        /// <summary>Mark appointment as completed (Doctor only)</summary>
        [HttpPatch("{id}/complete")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Complete(int id, [FromBody] UpdateStatusDto? dto)
        {
            var appt = await GetDoctorAppointment(id);
            if (appt == null) return NotFound(new { message = "Appointment not found or not yours." });
            if (appt.Status != AppointmentStatus.Confirmed) return BadRequest(new { message = "Only Confirmed appointments can be marked as completed." });

            appt.Status      = AppointmentStatus.Completed;
            appt.DoctorNotes = dto?.DoctorNotes ?? appt.DoctorNotes;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Doctor completed appointment {AppointmentId}", id);
            return Ok(new { message = "Appointment marked as completed." });
        }

        // ── Admin ────────────────────────────────────────────────────────

        /// <summary>Get all appointments (Admin only)</summary>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var appts = await BaseQuery()
                .OrderByDescending(a => a.BookedAt)
                .ToListAsync();
            return Ok(appts.Select(MapToDto));
        }

        // ── Private ──────────────────────────────────────────────────────

        private async Task<Appointment?> GetDoctorAppointment(int appointmentId)
        {
            var profileIdClaim = User.FindFirst("ProfileId")?.Value;
            if (!int.TryParse(profileIdClaim, out int doctorId)) return null;

            return await _context.Appointments
                .Include(a => a.Availability)
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.DoctorId == doctorId);
        }
    }
}
