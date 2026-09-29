using HealthcareApp.API.Data;
using HealthcareApp.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthcareApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DoctorsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Get all doctors (optionally filter by specializationId)</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? specializationId)
        {
            var query = _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialization)
                .AsQueryable();

            if (specializationId.HasValue)
                query = query.Where(d => d.SpecializationId == specializationId.Value);

            var doctors = await query
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

        /// <summary>Get a single doctor by ID</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialization)
                .Where(d => d.Id == id)
                .Select(d => new DoctorListDto
                {
                    Id                 = d.Id,
                    FullName           = d.User.FullName,
                    SpecializationName = d.Specialization.Name,
                    SpecializationId   = d.SpecializationId,
                    Phone              = d.Phone,
                    Bio                = d.Bio
                })
                .FirstOrDefaultAsync();

            if (doctor == null) return NotFound(new { message = "Doctor not found." });
            return Ok(doctor);
        }
    }
}
