using HealthcareApp.API.Data;
using HealthcareApp.API.DTOs;
using HealthcareApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HealthcareApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AvailabilityController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AvailabilityController> _logger;

        public AvailabilityController(AppDbContext context, ILogger<AvailabilityController> logger)
        {
            _context = context;
            _logger  = logger;
        }

        /// <summary>Get available (not booked) slots for a doctor</summary>
        [HttpGet("{doctorId}")]
        public async Task<IActionResult> GetByDoctor(int doctorId)
        {
            var rawSlots = await _context.Availabilities
                .Where(a => a.DoctorId == doctorId && !a.IsBooked && a.Date >= DateTime.Today)
                .ToListAsync();

            var slots = rawSlots
                .OrderBy(a => a.Date).ThenBy(a => a.StartTime)
                .Select(a => new AvailabilityDto
                {
                    Id        = a.Id,
                    DoctorId  = a.DoctorId,
                    Date      = a.Date,
                    StartTime = a.StartTime.ToString(@"hh\:mm"),
                    EndTime   = a.EndTime.ToString(@"hh\:mm"),
                    IsBooked  = a.IsBooked
                })
                .ToList();

            return Ok(slots);
        }

        /// <summary>Get ALL slots for a doctor (including booked) - Doctor view</summary>
        [HttpGet("{doctorId}/all")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> GetAllByDoctor(int doctorId)
        {
            var rawSlots = await _context.Availabilities
                .Where(a => a.DoctorId == doctorId)
                .ToListAsync();

            var slots = rawSlots
                .OrderBy(a => a.Date).ThenBy(a => a.StartTime)
                .Select(a => new AvailabilityDto
                {
                    Id        = a.Id,
                    DoctorId  = a.DoctorId,
                    Date      = a.Date,
                    StartTime = a.StartTime.ToString(@"hh\:mm"),
                    EndTime   = a.EndTime.ToString(@"hh\:mm"),
                    IsBooked  = a.IsBooked
                })
                .ToList();

            return Ok(slots);
        }

        /// <summary>Add a new availability slot (Doctor only)</summary>
        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> AddSlot([FromBody] CreateAvailabilityDto dto)
        {
            var profileIdClaim = User.FindFirst("ProfileId")?.Value;
            if (!int.TryParse(profileIdClaim, out int doctorId))
                return Unauthorized(new { message = "Doctor profile not found in token." });

            if (!TimeSpan.TryParse(dto.StartTime, out var start) || !TimeSpan.TryParse(dto.EndTime, out var end))
                return BadRequest(new { message = "Invalid time format. Use HH:mm." });

            if (end <= start)
                return BadRequest(new { message = "End time must be after start time." });

            if (dto.Date.Date < DateTime.Today)
                return BadRequest(new { message = "Cannot add slots in the past." });

            // Check overlap
            var existingSlots = await _context.Availabilities
                .Where(a => a.DoctorId == doctorId && a.Date.Date == dto.Date.Date)
                .ToListAsync();

            bool overlap = existingSlots.Any(a => a.StartTime < end && a.EndTime > start);
            if (overlap) return BadRequest(new { message = "This time slot overlaps with an existing slot." });

            var slot = new Availability
            {
                DoctorId  = doctorId,
                Date      = dto.Date,
                StartTime = start,
                EndTime   = end,
                IsBooked  = false
            };
            _context.Availabilities.Add(slot);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Doctor {DoctorId} added slot on {Date} at {Start}", doctorId, dto.Date.ToShortDateString(), start);

            return Ok(new AvailabilityDto
            {
                Id = slot.Id, DoctorId = slot.DoctorId, Date = slot.Date,
                StartTime = start.ToString(@"hh\:mm"), EndTime = end.ToString(@"hh\:mm"), IsBooked = false
            });
        }

        /// <summary>Delete a slot (Doctor only, must not be booked)</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> DeleteSlot(int id)
        {
            var profileIdClaim = User.FindFirst("ProfileId")?.Value;
            if (!int.TryParse(profileIdClaim, out int doctorId))
                return Unauthorized();

            var slot = await _context.Availabilities.FindAsync(id);
            if (slot == null) return NotFound(new { message = "Slot not found." });
            if (slot.DoctorId != doctorId) return Forbid();
            if (slot.IsBooked) return BadRequest(new { message = "Cannot delete a booked slot." });

            _context.Availabilities.Remove(slot);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Slot deleted." });
        }
    }
}
