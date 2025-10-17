using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessengerApp.Data;
using MessengerApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace MessengerApp.Controllers.Api
{
    [Route("api/user")]
    [ApiController]
    [Authorize] // 🔐 теперь всё API защищено — требует логин
    public class UserApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserApiController(AppDbContext context)
        {
            _context = context;
        }

        // -------------------- GET: api/user --------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .ToListAsync();
        }

        // -------------------- GET: api/user/5 --------------------
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null || !user.IsActive)
                return NotFound();

            return user;
        }

        // -------------------- POST: api/user --------------------
        // Регистрация через API — разрешена без логина
        [AllowAnonymous] // ✅ можно вызывать без авторизации
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;
            user.ModerationStatus = "Pending"; // 🚦 новые — на модерации

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // -------------------- PUT: api/user/5 --------------------
        // Редактировать — можно только авторизованным
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User updatedUser)
        {
            if (id != updatedUser.Id)
                return BadRequest();

            var user = await _context.Users.FindAsync(id);
            if (user == null || !user.IsActive)
                return NotFound();

            user.Username = updatedUser.Username;
            user.DepartmentId = updatedUser.DepartmentId;
            user.PasswordHash = updatedUser.PasswordHash;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // -------------------- DELETE (soft delete): api/user/5 --------------------
        // Мягкое удаление — доступно только модераторам / администраторам
        [Authorize(Roles = "Admin,Moderator")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || !user.IsActive)
                return NotFound();

            user.IsActive = false;
            user.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
