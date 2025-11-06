using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessengerApp.Data;

namespace MessengerApp.Controllers
{
    public class ChatController : Controller
    {
        private readonly AppDbContext _context;

        public ChatController(AppDbContext context)
        {
            _context = context;
        }

        // Получить список чатов пользователя
        [HttpGet]
        public async Task<IActionResult> List(string username)
        {
            if (string.IsNullOrEmpty(username))
                return BadRequest("Не указан username.");

            var user = await _context.Users
                .Include(u => u.UserChats)
                    .ThenInclude(uc => uc.Chat)
                        .ThenInclude(c => c.UserChats)
                            .ThenInclude(uc => uc.User)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return NotFound("Пользователь не найден.");

            var chats = user.UserChats
                .Where(uc => uc.Chat.IsActive)
                .Select(uc =>
                {
                    var chat = uc.Chat;
                    string chatName;

                    if (!chat.IsGroup)
                    {
                        var otherUser = chat.UserChats
                            .Select(x => x.User)
                            .FirstOrDefault(x => x.Id != user.Id);
                        chatName = otherUser?.DisplayName ?? "Личный чат";
                    }
                    else chatName = chat.Name;

                    return new { id = chat.Id, name = chatName };
                })
                .OrderBy(c => c.name)
                .ToList();

            return Json(chats);
        }

        // Получить сообщения определённого чата
        [HttpGet]
        public async Task<IActionResult> Messages(int chatId)
        {
            if (chatId <= 0)
                return BadRequest("Некорректный chatId.");

            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.ChatId == chatId)
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    sender = m.Sender.DisplayName ?? m.Sender.Username,
                    content = m.Content,
                    time = m.SentAt.ToString("HH:mm")
                })
                .ToListAsync();

            return Json(messages);
        }

        // Получить список всех пользователей (для модалки)
        [HttpGet]
        public async Task<IActionResult> Users(string currentUsername)
        {
            var users = await _context.Users
                .Where(u => u.IsActive && u.Username != currentUsername)
                .Select(u => new
                {
                    username = u.Username,
                    displayName = u.DisplayName
                })
                .OrderBy(u => u.displayName)
                .ToListAsync();

            return Json(users);
        }

        // Создать личный чат
        [HttpPost]
        public async Task<IActionResult> CreatePrivateChat(string username1, string username2)
        {
            if (username1 == username2)
                return BadRequest("Нельзя создать чат с самим собой.");

            var user1 = await _context.Users.FirstOrDefaultAsync(u => u.Username == username1);
            var user2 = await _context.Users.FirstOrDefaultAsync(u => u.Username == username2);

            if (user1 == null || user2 == null)
                return NotFound("Пользователь не найден.");

            var existingChat = await _context.Chats
                .Include(c => c.UserChats)
                .FirstOrDefaultAsync(c => !c.IsGroup &&
                    c.UserChats.Any(uc => uc.UserId == user1.Id) &&
                    c.UserChats.Any(uc => uc.UserId == user2.Id));

            if (existingChat != null)
                return Json(new { id = existingChat.Id, name = user2.DisplayName });

            var chat = new Models.Chat
            {
                Name = $"ЛС: {user1.DisplayName} ↔ {user2.DisplayName}",
                IsGroup = false,
                IsPrivate = true,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            _context.UserChats.AddRange(
                new Models.UserChat { UserId = user1.Id, ChatId = chat.Id },
                new Models.UserChat { UserId = user2.Id, ChatId = chat.Id }
            );

            await _context.SaveChangesAsync();

            return Json(new { id = chat.Id, name = user2.DisplayName });
        }
    }
}
