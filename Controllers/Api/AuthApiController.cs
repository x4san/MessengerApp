using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessengerApp.Data;
using MessengerApp.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using System.Text;

namespace MessengerApp.Controllers.Api
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/auth")]
    public class AuthApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthApiController(AppDbContext context)
        {
            _context = context;
        }

        // Регистрация нового пользователя
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] string username, [FromForm] string password, [FromForm] string displayName)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return BadRequest(new { error = "Username and password are required" });

            if (await _context.Users.AnyAsync(u => u.Username == username))
                return Conflict(new { error = "User already exists" });

            var user = new User
            {
                Username = username,
                DisplayName = displayName,
                PasswordHash = ComputeHash(password),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered", user.Id, user.Username, user.DisplayName });
        }

        // Логин (Cookie создаёт MVC, здесь просто проверка)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] string username, [FromForm] string password)
        {
            var hash = ComputeHash(password);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == hash && u.IsActive);

            if (user == null)
                return Unauthorized(new { error = "Invalid credentials" });

            return Ok(new { message = "Login success", user.Id, user.Username, user.DisplayName });
        }

        private static string ComputeHash(string input)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(input)));
        }
    }
}
