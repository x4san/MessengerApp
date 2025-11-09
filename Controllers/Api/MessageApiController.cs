using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessengerApp.Data;
using MessengerApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace MessengerApp.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/messages")]
    public class MessageApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MessageApiController(AppDbContext context)
        {
            _context = context;
        }

        // Получить сообщения чата
        [HttpGet("{chatId}")]
        public async Task<IActionResult> GetMessages(int chatId)
        {
            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.ChatId == chatId && m.IsActive)
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    m.Id,
                    Sender = m.Sender.DisplayName,
                    m.Content,
                    Time = m.SentAt.ToString("HH:mm")
                })
                .ToListAsync();

            return Ok(messages);
        }

        // Отправить сообщение
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromForm] int chatId, [FromForm] string username, [FromForm] string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return BadRequest(new { error = "Message content is empty" });

            var sender = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (sender == null)
                return NotFound(new { error = "Sender not found" });

            var msg = new Message
            {
                ChatId = chatId,
                SenderId = sender.Id,
                Content = content,
                SentAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Message sent", msg.Id });
        }

        // Удалить сообщение (только админ)
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var msg = await _context.Messages.FindAsync(id);
            if (msg == null)
                return NotFound(new { error = "Message not found" });

            msg.IsActive = false;
            msg.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Message {id} deleted" });
        }
    }
}
