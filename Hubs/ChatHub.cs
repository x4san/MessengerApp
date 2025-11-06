using Microsoft.AspNetCore.SignalR;
using MessengerApp.Data;
using MessengerApp.Models;
using MessengerApp.Models.Dtos;
using MessengerApp.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace MessengerApp.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;
        private readonly ChatQueryService _chatQueryService;

        public ChatHub(AppDbContext context, ChatQueryService chatQueryService)
        {
            _context = context;
            _chatQueryService = chatQueryService;
        }

        // Регистрация пользователя и отправка списка чатов
        public async Task RegisterUser(string username)
        {
            var user = await _chatQueryService.GetUserByUsernameAsync(username);
            if (user == null)
                return;

            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{user.Username}");

            var chatIds = await _context.UserChats
                .Where(uc => uc.UserId == user.Id && uc.Chat.IsActive)
                .Select(uc => uc.ChatId)
                .ToListAsync();

            foreach (var chatId in chatIds)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatId}");
            }

            var chatSummaries = await _chatQueryService.GetChatSummariesAsync(user.Id);
            await Clients.Caller.SendAsync("ChatList", chatSummaries);
        }

        // Подключение к конкретному чату
        public async Task JoinChat(int chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatId}");
        }

        // Отправка сообщения в чат
        public async Task SendMessage(int chatId, string senderUsername, string content, int? replyToMessageId)
        {
            if (string.IsNullOrWhiteSpace(content))
                return;

            var sender = await _context.Users.FirstOrDefaultAsync(u => u.Username == senderUsername);
            if (sender == null)
                return;

            var userChat = await _context.UserChats
                .Include(uc => uc.Chat)
                    .ThenInclude(c => c.UserChats)
                        .ThenInclude(uc => uc.User)
                .FirstOrDefaultAsync(uc => uc.ChatId == chatId && uc.UserId == sender.Id);

            if (userChat == null)
                return;

            Message? replyMessage = null;

            if (replyToMessageId.HasValue)
            {
                replyMessage = await _context.Messages
                    .Include(m => m.Sender)
                    .FirstOrDefaultAsync(m => m.Id == replyToMessageId.Value && m.ChatId == chatId && m.IsActive);
            }

            // Создаём сообщение
            var message = new Message
            {
                ChatId = chatId,
                SenderId = sender.Id,
                Content = content.Trim(),
                ReplyToMessageId = replyMessage?.Id,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            userChat.LastReadMessageId = message.Id;
            userChat.LastReadAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var messageDto = await _chatQueryService.GetMessageDtoAsync(message.Id);
            if (messageDto == null)
                return;

            await Clients.Group($"chat_{chatId}")
                .SendAsync("ReceiveMessage", messageDto);

            foreach (var participant in userChat.Chat.UserChats)
            {
                var summary = await _chatQueryService.GetChatSummaryForUserAsync(chatId, participant.UserId);
                if (summary != null)
                {
                    await Clients.Group($"user_{participant.User.Username}")
                        .SendAsync("ChatUpdated", summary);
                }
            }
        }

        public async Task MarkAsRead(int chatId, string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return;

            var userChat = await _context.UserChats
                .FirstOrDefaultAsync(uc => uc.ChatId == chatId && uc.UserId == user.Id);

            if (userChat == null)
                return;

            var lastMessageId = await _context.Messages
                .Where(m => m.ChatId == chatId && m.IsActive)
                .OrderByDescending(m => m.SentAt)
                .ThenByDescending(m => m.Id)
                .Select(m => m.Id)
                .FirstOrDefaultAsync();

            if (lastMessageId == 0)
                return;

            if (userChat.LastReadMessageId == lastMessageId)
                return;

            userChat.LastReadMessageId = lastMessageId;
            userChat.LastReadAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var summary = await _chatQueryService.GetChatSummaryForUserAsync(chatId, user.Id);
            if (summary != null)
            {
                await Clients.Group($"user_{username}")
                    .SendAsync("ChatUpdated", summary);
            }
        }

        public async Task EditMessage(int messageId, string username, string newContent)
        {
            if (string.IsNullOrWhiteSpace(newContent))
                return;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return;

            var message = await _context.Messages
                .Include(m => m.Chat)
                    .ThenInclude(c => c.UserChats)
                        .ThenInclude(uc => uc.User)
                .Include(m => m.Sender)
                .Include(m => m.ReplyToMessage)
                    .ThenInclude(r => r.Sender)
                .FirstOrDefaultAsync(m => m.Id == messageId && m.IsActive);

            if (message == null || message.SenderId != user.Id)
                return;

            message.Content = newContent.Trim();
            message.IsEdited = true;

            await _context.SaveChangesAsync();

            var messageDto = await _chatQueryService.GetMessageDtoAsync(message.Id);
            if (messageDto == null)
                return;

            await Clients.Group($"chat_{message.ChatId}")
                .SendAsync("MessageEdited", messageDto);

            foreach (var participant in message.Chat.UserChats)
            {
                var summary = await _chatQueryService.GetChatSummaryForUserAsync(message.ChatId, participant.UserId);
                if (summary != null)
                {
                    await Clients.Group($"user_{participant.User.Username}")
                        .SendAsync("ChatUpdated", summary);
                }
            }
        }
    }
}
