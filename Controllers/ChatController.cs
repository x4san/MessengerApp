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

            // Находим пользователя и подгружаем его чаты с участниками
            var user = await _context.Users
                .Include(u => u.UserChats)
                    .ThenInclude(uc => uc.Chat)
                        .ThenInclude(c => c.UserChats)
                            .ThenInclude(uc => uc.User)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return NotFound("Пользователь не найден.");

            // Формируем список чатов
            var chats = user.UserChats
                .Where(uc => uc.Chat.IsActive)
                .Select(uc =>
                {
                    var chat = uc.Chat;

                    // Если это личный чат → показываем имя собеседника
                    string chatName;
                    if (!chat.IsGroup)
                    {
                        var otherUser = chat.UserChats
                            .Select(x => x.User)
                            .FirstOrDefault(x => x.Id != user.Id);

                        chatName = otherUser?.DisplayName ?? "Личный чат";
                    }
                    else
                    {
                        chatName = chat.Name;
                    }

                    return new
                    {
                        id = chat.Id,
                        name = chatName
                    };
                })
                // Можно добавить лёгкую сортировку
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
    }
}
