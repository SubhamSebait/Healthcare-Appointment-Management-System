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
    public class SpecializationsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SpecializationsController> _logger;

        public SpecializationsController(AppDbContext context, ILogger<SpecializationsController> logger)
        {
            _context = context;
            _logger  = logger;
        }

        /// <summary>Get all specializations (public)</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var specs = await _context.Specializations
                .Select(s => new SpecializationDto { Id = s.Id, Name = s.Name, Description = s.Description })
                .ToListAsync();
            return Ok(specs);
        }

        /// <summary>Get specialization by ID (public)</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var s = await _context.Specializations.FindAsync(id);
            if (s == null) return NotFound(new { message = "Specialization not found." });
            return Ok(new SpecializationDto { Id = s.Id, Name = s.Name, Description = s.Description });
        }

        /// <summary>Create specialization (Admin only)</summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateSpecializationDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Name is required." });

            var spec = new Specialization { Name = dto.Name, Description = dto.Description };
            _context.Specializations.Add(spec);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Admin created specialization: {Name}", dto.Name);
            return CreatedAtAction(nameof(GetById), new { id = spec.Id }, new SpecializationDto { Id = spec.Id, Name = spec.Name, Description = spec.Description });
        }

        /// <summary>Update specialization (Admin only)</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateSpecializationDto dto)
        {
            var spec = await _context.Specializations.FindAsync(id);
            if (spec == null) return NotFound(new { message = "Specialization not found." });

            spec.Name        = dto.Name;
            spec.Description = dto.Description;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Admin updated specialization {Id}: {Name}", id, dto.Name);
            return Ok(new SpecializationDto { Id = spec.Id, Name = spec.Name, Description = spec.Description });
        }

        /// <summary>Delete specialization (Admin only)</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var spec = await _context.Specializations.Include(s => s.Doctors).FirstOrDefaultAsync(s => s.Id == id);
            if (spec == null) return NotFound(new { message = "Specialization not found." });
            if (spec.Doctors.Count > 0) return BadRequest(new { message = "Cannot delete specialization that has doctors assigned." });

            _context.Specializations.Remove(spec);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Admin deleted specialization {Id}", id);
            return Ok(new { message = "Specialization deleted." });
        }
    }
}
