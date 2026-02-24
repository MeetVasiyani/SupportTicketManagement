using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SupportTicketManagementAPI.Controllers
{
    [Route("users")]
    [ApiController]
    [Authorize(Roles = "MANAGER")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var role = _context.Roles.FirstOrDefault(r => r.Name == dto.role);
            if (role == null)
            {
                return BadRequest("Invalid role.");
            }

            var exists = await _context.Users.AnyAsync(u => u.Email == dto.email);
            if (exists)
            {
                return BadRequest("Email already exists.");
            }

            var hashedpass = BCrypt.Net.BCrypt.HashPassword(dto.password);

            var user = new User()
            {
                Name = dto.name,
                Email = dto.email,
                Password = hashedpass,
                RoleId = role.Id
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var response = new
            {
                id = user.Id,
                name = user.Name,
                email = user.Email,
                role = new{id = role.Id,name = role.Name.ToString()},
                created_at = user.CreatedAt
            };

            return StatusCode(201,response);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.Include(u => u.Role).Select(u => new
            {
                id = u.Id,
                name = u.Name,
                email = u.Email,
                role = new{id = u.Role.Id,name = u.Role.Name.ToString()},
                created_at = u.CreatedAt
            }).ToListAsync();

            return Ok(users);
        }
    }
}
