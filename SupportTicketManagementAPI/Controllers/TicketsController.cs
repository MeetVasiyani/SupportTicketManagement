using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace SupportTicketManagementAPI.Controllers
{
    [Route("tickets")]
    [ApiController]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public TicketsController(AppDbContext context)
        {
            _context = context;
        }

        [NonAction]
        public int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [NonAction]
        public string GetUserrole()
        {
            return User.FindFirstValue(ClaimTypes.Role)!;
        }

        [HttpPost]
        [Authorize(Roles = "USER,MANAGER")]
        public async Task<IActionResult> CreateTicket(CreateTicketDTO dto)
        {
            var userId = GetUserId();

            var ticket = new Ticket()
            {
                Title = dto.title,
                Description = dto.description,
                Priority = dto.priority,
                Status = TicketsStatus.OPEN,
                CreatedBy = userId
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            var created = await _context.Tickets.Include(t => t.CreatingUser)
            .ThenInclude(u => u.Role).Include(t => t.AssignedUser).ThenInclude(u => u.Role).FirstAsync(t => t.Id == ticket.Id);

            return StatusCode(201, MapResponse(created));
        }

        [HttpGet]
        public async Task<IActionResult> GetTickets()
        {
            var userId = GetUserId();
            var role = GetUserrole();

            IQueryable<Ticket> tickets = _context.Tickets.Include(t => t.CreatingUser)
            .ThenInclude(u => u.Role).Include(t => t.AssignedUser).ThenInclude(u => u.Role);

            if (role == "SUPPORT")
            {
                tickets = tickets.Where(t => t.AssignedTo == userId);
            }
            else if (role == "USER")
            {
                tickets = tickets.Where(t => t.CreatedBy == userId);
            }

            var res = await tickets.ToListAsync();

            return Ok(res.Select(MapResponse));
        }

        [HttpPatch("{id}/assign")]
        [Authorize(Roles = "MANAGER,SUPPORT")]
        public async Task<IActionResult> AssignTickets(int id, AssignDTO dto)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == dto.userId);
            if (user == null)
            {
                return BadRequest();
            }
            var role = GetUserrole();

            if (role == "SUPPORT")
            {
                var curruserId = GetUserId();

                if (ticket.AssignedTo != null && ticket.AssignedTo != curruserId)
                {
                    return Forbid();
                }
            }
            if (user.Role.Name == RoleTypes.USER)
            {
                return BadRequest();
            }

            ticket.AssignedTo = user.Id;

            await _context.SaveChangesAsync();

            var updates = await _context.Tickets.Include(t => t.CreatingUser)
            .ThenInclude(u => u.Role).Include(t => t.AssignedUser).ThenInclude(u => u.Role).FirstAsync(t => t.Id == id);

            return Ok(MapResponse(updates));
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "MANAGER,SUPPORT")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateStatusDTO dto)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            if (!IsValidTransition(ticket.Status, dto.status))
            {
                return BadRequest("Invalid status transition.");
            }

            var role = GetUserrole();
            if (role == "SUPPORT" && ticket.AssignedTo != GetUserId())
            {
                return Forbid();
            }

            var log = new TicketStatusLog
            {
                TicketId = ticket.Id,
                OldStatus = ticket.Status,
                NewStatus = dto.status,
                ChangedBy = GetUserId()
            };

            ticket.Status = dto.status;

            _context.TicketStatusLogs.Add(log);
            await _context.SaveChangesAsync();

            var updates = await _context.Tickets.Include(t => t.CreatingUser)
            .ThenInclude(u => u.Role).Include(t => t.AssignedUser).ThenInclude(u => u.Role).FirstAsync(t => t.Id == id);

            return Ok(MapResponse(updates));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "MANAGER")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/comments")]
        public async Task<IActionResult> AddComment(int id, CommentDTO dto)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            if (!CanHaveTicket(ticket))
            {
                return Forbid();
            }

            var comment = new TicketComment
            {
                TicketId = id,
                UserId = GetUserId(),
                Comment = dto.comment
            };

            _context.TicketComments.Add(comment);
            await _context.SaveChangesAsync();

            var created = await _context.TicketComments.Include(c => c.User).ThenInclude(u => u.Role).FirstAsync(c => c.Id == comment.Id);

            return StatusCode(201, MaptoComment(created));
        }

        [HttpGet("{id}/comments")]
        public async Task<IActionResult> GetComment(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            if (!CanHaveTicket(ticket))
            {
                return Forbid();
            }

            var comment = await _context.TicketComments.Where(c => c.TicketId == id).Include(c => c.User).ThenInclude(u => u.Role).ToListAsync();

            return Ok(comment.Select(MaptoComment));
        }

        [HttpPatch("comments/{id}")]
        public async Task<IActionResult> EditComment(int id, CommentDTO dto)
        {
            var comment = await _context.TicketComments.Include(c => c.User).ThenInclude(u => u.Role).FirstOrDefaultAsync(c => c.Id == id);
            if(comment == null)
            {
                return NotFound();
            }

            var role = GetUserrole();
            var userId = GetUserId();

            if (role != "MANAGER" && comment.UserId != userId)
            {
                return Forbid();
            }

            comment.Comment = dto.comment;
            await _context.SaveChangesAsync();
            return Ok(MaptoComment(comment));

        }

        [HttpDelete("/comments/{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.TicketComments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            var role = GetUserrole();
            var userId = GetUserId();

            if (role != "MANAGER" && comment.UserId != userId)
            {
                return Forbid();
            }

            _context.TicketComments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CanHaveTicket(Ticket ticket)
        {
            var role = GetUserrole();
            var userId = GetUserId();

            if (role == "MANAGER") return true;
            if (role == "SUPPORT" && ticket.AssignedTo == userId) return true;
            if (role == "USER" && ticket.CreatedBy == userId) return true;
            return false;
        }
        private bool IsValidTransition(TicketsStatus current, TicketsStatus next)
        {
            var validMoves = new (TicketsStatus From, TicketsStatus To)[]
            {(TicketsStatus.OPEN,        TicketsStatus.IN_PROGRESS),
            (TicketsStatus.IN_PROGRESS, TicketsStatus.RESOLVED),
            (TicketsStatus.RESOLVED,    TicketsStatus.CLOSED)};

            return validMoves.Contains((current, next));
        }

        private object MaptoComment(TicketComment c)
        {
            return new
            {
                id = c.Id,
                comment = c.Comment,
                user = new{
                    id = c.User.Id,
                    name = c.User.Name,
                    email = c.User.Email,
                    role = new{
                        id = c.User.Role.Id,name = c.User.Role.Name.ToString()},
                    created_at = c.User.CreatedAt
                },
                created_at = c.CreatedAt
            };
        }
        private object MapResponse(Ticket t)
        {
            return new
            {
                id = t.Id,
                title = t.Title,
                description = t.Description,
                status = t.Status.ToString(),
                priority = t.Priority.ToString(),
                created_by = new
                {
                    id = t.CreatingUser.Id,
                    name = t.CreatingUser.Name,
                    email = t.CreatingUser.Email,
                    role = new
                    {
                        id = t.CreatingUser.Role.Id,
                        name = t.CreatingUser.Role.Name.ToString()
                    },

                    created_at = t.CreatingUser.CreatedAt
                },
                assigned_to = t.AssignedUser == null ? null : new
                {
                    id = t.AssignedUser.Id,
                    name = t.AssignedUser.Name,
                    email = t.AssignedUser.Email,
                    role = new
                    {
                        id = t.AssignedUser.Role.Id,
                        name = t.AssignedUser.Role.Name.ToString()
                    },
                    created_at = t.AssignedUser.CreatedAt
                },

                created_at = t.CreatedAt
            };
        }
    }
}
