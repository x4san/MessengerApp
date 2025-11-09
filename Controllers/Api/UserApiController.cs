using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessengerApp.Data;
using Microsoft.AspNetCore.Authorization;

namespace MessengerApp.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/user")]
    public class UserApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserApiController(AppDbContext context)
        {
            _context = context;
        }

        // Все активные пользователи
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _context.Users
                .Where(u => u.IsActive)
                .Select(u => new { u.Id, u.Username, u.DisplayName })
                .ToListAsync();

            return Ok(users);
        }

        // Конкретный пользователь
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _context.Users
                .Where(u => u.IsActive && u.Id == id)
                .Select(u => new { u.Id, u.Username, u.DisplayName })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { error = "User not found" });

            return Ok(user);
        }

        // Деактивация (только админ)
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { error = "User not found" });

            user.IsActive = false;
            user.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"User {user.Username} deactivated" });
        }
    }
}
