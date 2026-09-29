using HealthcareApp.API.DTOs;
using HealthcareApp.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>Register a new patient account</summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "Email and password are required." });

            var (success, message, result) = await _authService.RegisterPatientAsync(dto);
            if (!success) return BadRequest(new { message });
            return Ok(result);
        }

        /// <summary>Login for all roles (Patient, Doctor, Admin)</summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "Email and password are required." });

            var (success, message, result) = await _authService.LoginAsync(dto);
            if (!success) return Unauthorized(new { message });
            return Ok(result);
        }
    }
}
