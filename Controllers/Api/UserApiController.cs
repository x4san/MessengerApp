using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessengerApp.Data;
using MessengerApp.Models;

namespace MessengerApp.Controllers.Api
{
    [Route("api/user")]
    [ApiController]
    public class UserApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserApiController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/user
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .ToListAsync();
        }

        // GET: api/user/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null || !user.IsActive)
                return NotFound();

            return user;
        }

        // POST: api/user
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // PUT: api/user/5
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

        // DELETE (soft delete): api/user/5
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
