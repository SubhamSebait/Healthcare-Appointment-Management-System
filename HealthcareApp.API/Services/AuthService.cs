using HealthcareApp.API.Data;
using HealthcareApp.API.DTOs;
using HealthcareApp.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HealthcareApp.API.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext context, IConfiguration config, ILogger<AuthService> logger)
        {
            _context = context;
            _config  = config;
            _logger  = logger;
        }

        public async Task<(bool success, string message, AuthResponseDto? result)> RegisterPatientAsync(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return (false, "Email already registered.", null);

            var user = new User
            {
                FullName     = dto.FullName,
                Email        = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role         = "Patient"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var patient = new Patient
            {
                UserId      = user.Id,
                Phone       = dto.Phone,
                DateOfBirth = dto.DateOfBirth
            };
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            _logger.LogInformation("New patient registered: {Email}", dto.Email);

            var token = GenerateToken(user, patient.Id);
            return (true, "Registration successful.", new AuthResponseDto
            {
                Token     = token,
                Role      = user.Role,
                FullName  = user.FullName,
                UserId    = user.Id,
                ProfileId = patient.Id
            });
        }

        public async Task<(bool success, string message, AuthResponseDto? result)> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Patient)
                .Include(u => u.Doctor)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for email: {Email}", dto.Email);
                return (false, "Invalid email or password.", null);
            }

            int? profileId = user.Role switch
            {
                "Patient" => user.Patient?.Id,
                "Doctor"  => user.Doctor?.Id,
                _         => null
            };

            _logger.LogInformation("User logged in: {Email} (Role: {Role})", user.Email, user.Role);

            var token = GenerateToken(user, profileId);
            return (true, "Login successful.", new AuthResponseDto
            {
                Token     = token,
                Role      = user.Role,
                FullName  = user.FullName,
                UserId    = user.Id,
                ProfileId = profileId
            });
        }

        private string GenerateToken(User user, int? profileId)
        {
            var jwtKey    = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured");
            var key       = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds     = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAt = DateTime.UtcNow.AddHours(8);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role),
            };

            if (profileId.HasValue)
                claims.Add(new Claim("ProfileId", profileId.Value.ToString()));

            var token = new JwtSecurityToken(
                issuer:            _config["Jwt:Issuer"],
                audience:          _config["Jwt:Audience"],
                claims:            claims,
                expires:           expiresAt,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
