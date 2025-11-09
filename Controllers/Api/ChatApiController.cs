using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessengerApp.Data;
using MessengerApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace MessengerApp.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/chats")]
    public class ChatApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChatApiController(AppDbContext context)
        {
            _context = context;
        }

        // Список чатов пользователя
        [HttpGet("{username}")]
        public async Task<IActionResult> GetUserChats(string username)
        {
            var user = await _context.Users
                .Include(u => u.UserChats)
                    .ThenInclude(uc => uc.Chat)
                        .ThenInclude(c => c.UserChats)
                            .ThenInclude(uc => uc.User)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return NotFound(new { error = "User not found" });

            var chats = user.UserChats
                .Where(uc => uc.Chat.IsActive)
                .Select(uc =>
                {
                    var chat = uc.Chat;
                    string name;
                    if (chat.IsGroup)
                        name = chat.Name;
                    else
                    {
                        var other = chat.UserChats.Select(x => x.User).FirstOrDefault(x => x.Id != user.Id);
                        name = other?.DisplayName ?? "Личный чат";
                    }

                    return new { chat.Id, chat.IsGroup, chat.IsPrivate, name };
                })
                .ToList();

            return Ok(chats);
        }

        // Создать личный чат
        [HttpPost("private")]
        public async Task<IActionResult> CreatePrivate([FromForm] string username1, [FromForm] string username2)
        {
            if (username1 == username2)
                return BadRequest(new { error = "Cannot create chat with yourself" });

            var u1 = await _context.Users.FirstOrDefaultAsync(u => u.Username == username1);
            var u2 = await _context.Users.FirstOrDefaultAsync(u => u.Username == username2);

            if (u1 == null || u2 == null)
                return NotFound(new { error = "User not found" });

            var existing = await _context.Chats
                .Include(c => c.UserChats)
                .FirstOrDefaultAsync(c => !c.IsGroup &&
                    c.UserChats.Any(uc => uc.UserId == u1.Id) &&
                    c.UserChats.Any(uc => uc.UserId == u2.Id));

            if (existing != null)
                return Ok(new { existing.Id, name = u2.DisplayName });

            var chat = new Chat
            {
                Name = $"ЛС: {u1.DisplayName} ↔ {u2.DisplayName}",
                IsGroup = false,
                IsPrivate = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            _context.UserChats.AddRange(
                new UserChat { UserId = u1.Id, ChatId = chat.Id },
                new UserChat { UserId = u2.Id, ChatId = chat.Id }
            );

            await _context.SaveChangesAsync();

            return Ok(new { chat.Id, name = u2.DisplayName });
        }
    }
}
