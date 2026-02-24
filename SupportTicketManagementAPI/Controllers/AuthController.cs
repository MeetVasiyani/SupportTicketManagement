using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace SupportTicketManagementAPI.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        public AuthController(IConfiguration configuration, AppDbContext context)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == dto.email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.password, user.Password))
            {
                return Unauthorized();
            }

            var token = GenerateJwtTokens(user);

            return Ok(new AuthResponse
            {
                token = token,
            });
        }

        private string GenerateJwtTokens(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]{
              new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
              new Claim(ClaimTypes.Email, user.Email),
              new Claim(ClaimTypes.Role, user.Role.Name.ToString())
            };

            var token = new JwtSecurityToken (
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes (
                    Convert.ToDouble(jwtSettings["DurationInMinutes"] ?? "60")
                ),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
