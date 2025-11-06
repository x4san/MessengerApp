using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessengerApp.Data;
using MessengerApp.Hubs;
using MessengerApp.Services;
using Microsoft.AspNetCore.SignalR;
using System.Linq;

namespace MessengerApp.Controllers
{
    public class ChatController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _chatHubContext;
        private readonly ChatQueryService _chatQueryService;

        public ChatController(AppDbContext context, IHubContext<ChatHub> chatHubContext, ChatQueryService chatQueryService)
        {
            _context = context;
            _chatHubContext = chatHubContext;
            _chatQueryService = chatQueryService;
        }

        // Получить список чатов пользователя
        [HttpGet]
        public async Task<IActionResult> List(string username)
        {
            if (string.IsNullOrEmpty(username))
                return BadRequest("Не указан username.");

            var user = await _chatQueryService.GetUserByUsernameAsync(username);

            if (user == null)
                return NotFound("Пользователь не найден.");

            var chats = await _chatQueryService.GetChatSummariesAsync(user.Id);
            return Json(chats);
        }

        // Получить сообщения определённого чата
        [HttpGet]
        public async Task<IActionResult> Messages(int chatId, string username)
        {
            if (chatId <= 0)
                return BadRequest("Некорректный chatId.");

            var user = await _chatQueryService.GetUserByUsernameAsync(username);
            if (user == null)
                return NotFound("Пользователь не найден.");

            var hasAccess = await _context.UserChats
                .AnyAsync(uc => uc.ChatId == chatId && uc.UserId == user.Id);

            if (!hasAccess)
                return Forbid();

            var messages = await _chatQueryService.GetMessagesForChatAsync(chatId);
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

            var summaryForUser1 = await _chatQueryService.GetChatSummaryForUserAsync(chat.Id, user1.Id);
            var summaryForUser2 = await _chatQueryService.GetChatSummaryForUserAsync(chat.Id, user2.Id);

            if (summaryForUser1 != null)
            {
                await _chatHubContext.Clients.Group($"user_{user1.Username}")
                    .SendAsync("ChatUpdated", summaryForUser1);
            }

            if (summaryForUser2 != null)
            {
                await _chatHubContext.Clients.Group($"user_{user2.Username}")
                    .SendAsync("ChatUpdated", summaryForUser2);
            }

            return Json(new
            {
                chatId = chat.Id,
                userOne = summaryForUser1,
                userTwo = summaryForUser2
            });
        }
    }
}
