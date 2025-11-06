using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessengerApp.Data;
using MessengerApp.Hubs;
using MessengerApp.Services;
using MessengerApp.Utils;
using Microsoft.AspNetCore.SignalR;

namespace MessengerApp.Controllers
{
    public class ChatController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ChatPresentationService _presentationService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(AppDbContext context, ChatPresentationService presentationService, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _presentationService = presentationService;
            _hubContext = hubContext;
        }

        // Получить список чатов пользователя
        [HttpGet]
        public async Task<IActionResult> List(string username)
        {
            if (string.IsNullOrEmpty(username))
                return BadRequest("Не указан username.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return NotFound("Пользователь не найден.");

            var chatSummaries = await _presentationService.GetChatSummariesAsync(user.Id);

            var result = chatSummaries.Select(c => new
            {
                id = c.Id,
                name = c.Name,
                lastMessage = c.LastMessage,
                lastSender = c.LastSender,
                lastMessageTime = c.LastMessageUtc?.ToLocalTime().ToString("HH:mm"),
                lastMessageUtc = c.LastMessageUtc,
                lastMessageId = c.LastMessageId,
                unreadCount = c.UnreadCount,
                isGroup = c.IsGroup,
                avatarInitials = c.AvatarInitials,
                avatarColor = c.AvatarColor
            });

            return Json(result);
        }

        // Получить сообщения определённого чата
        [HttpGet]
        public async Task<IActionResult> Messages(int chatId, string username)
        {
            if (chatId <= 0)
                return BadRequest("Некорректный chatId.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return NotFound("Пользователь не найден.");

            var membership = await _context.UserChats.FirstOrDefaultAsync(uc => uc.ChatId == chatId && uc.UserId == user.Id);
            if (membership == null)
                return Forbid();

            var messages = await _presentationService.GetMessagesAsync(chatId);

            var result = messages.Select(m => new
            {
                id = m.Id,
                chatId = m.ChatId,
                senderId = m.SenderId,
                senderUsername = m.SenderUsername,
                sender = m.Sender,
                avatarInitials = m.AvatarInitials,
                avatarColor = m.AvatarColor,
                content = m.Content,
                sentAt = m.SentAtUtc,
                time = m.Time,
                isOwn = m.SenderId == user.Id,
                isEdited = m.IsEdited,
                reply = m.Reply == null ? null : new
                {
                    id = m.Reply.Id,
                    sender = m.Reply.Sender,
                    snippet = m.Reply.Snippet
                }
            });

            return Json(result);
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
            {
                var summary = await _presentationService.GetChatSummaryAsync(existingChat.Id, user1.Id);
                return Json(summary);
            }

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

            var now = DateTime.UtcNow;
            _context.UserChats.AddRange(
                new Models.UserChat { UserId = user1.Id, ChatId = chat.Id, LastReadAt = now },
                new Models.UserChat { UserId = user2.Id, ChatId = chat.Id, LastReadAt = now }
            );

            await _context.SaveChangesAsync();

            var summaryForUser1 = await _presentationService.GetChatSummaryAsync(chat.Id, user1.Id);
            var summaryForUser2 = await _presentationService.GetChatSummaryAsync(chat.Id, user2.Id);

            if (summaryForUser1 != null)
            {
                await _hubContext.Clients.Group($"user_{user1.Id}").SendAsync("ChatUpdated", summaryForUser1);
            }

            if (summaryForUser2 != null)
            {
                await _hubContext.Clients.Group($"user_{user2.Id}").SendAsync("ChatUpdated", summaryForUser2);
            }

            return Json(summaryForUser1);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string displayName, string? bio)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                return BadRequest("Имя не может быть пустым");

            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var user = await _context.Users
                .Include(u => u.UserChats)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return NotFound("Пользователь не найден");

            user.DisplayName = displayName.Trim();
            user.Bio = string.IsNullOrWhiteSpace(bio) ? null : bio.Trim();
            await _context.SaveChangesAsync();

            var chatIds = user.UserChats.Select(uc => uc.ChatId).ToList();
            foreach (var chatId in chatIds)
            {
                var participants = await _context.UserChats
                    .Where(uc => uc.ChatId == chatId)
                    .Select(uc => uc.UserId)
                    .ToListAsync();

                foreach (var participant in participants)
                {
                    var summary = await _presentationService.GetChatSummaryAsync(chatId, participant);
                    if (summary != null)
                    {
                        await _hubContext.Clients.Group($"user_{participant}").SendAsync("ChatUpdated", summary);
                    }
                }
            }

            return Json(new
            {
                displayName = user.DisplayName,
                bio = user.Bio,
                initials = ChatFormattingHelper.BuildInitials(user.DisplayName),
                avatarColor = ChatFormattingHelper.PickColor(user.Username)
            });
        }
    }
}
