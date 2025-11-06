using MessengerApp.Data;
using MessengerApp.Models;
using MessengerApp.Models.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace MessengerApp.Services
{
    public class ChatQueryService
    {
        private readonly AppDbContext _context;

        public ChatQueryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
        }

        public async Task<List<ChatSummaryDto>> GetChatSummariesAsync(int userId)
        {
            var userChats = await _context.UserChats
                .AsNoTracking()
                .Include(uc => uc.Chat)
                    .ThenInclude(c => c.UserChats)
                        .ThenInclude(uc => uc.User)
                .Where(uc => uc.UserId == userId && uc.Chat.IsActive)
                .ToListAsync();

            var chatIds = userChats.Select(uc => uc.ChatId).Distinct().ToList();

            var messageSnapshots = await _context.Messages
                .AsNoTracking()
                .Where(m => chatIds.Contains(m.ChatId) && m.IsActive)
                .Select(m => new
                {
                    m.ChatId,
                    m.Id,
                    m.Content,
                    m.SentAt,
                    SenderDisplayName = m.Sender.DisplayName ?? m.Sender.Username,
                    m.SenderId
                })
                .ToListAsync();

            var summaries = new List<ChatSummaryDto>();

            foreach (var uc in userChats)
            {
                var chat = uc.Chat;
                var title = BuildChatTitle(chat, userId);
                var lastReadId = uc.LastReadMessageId ?? 0;

                var lastMessage = messageSnapshots
                    .Where(m => m.ChatId == chat.Id)
                    .OrderByDescending(m => m.SentAt)
                    .ThenByDescending(m => m.Id)
                    .FirstOrDefault();

                var unreadCount = messageSnapshots
                    .Where(m => m.ChatId == chat.Id && m.Id > lastReadId && m.SenderId != userId)
                    .Count();

                var dto = new ChatSummaryDto
                {
                    ChatId = chat.Id,
                    Title = title,
                    IsGroup = chat.IsGroup,
                    LastMessagePreview = lastMessage != null ? BuildPreview(lastMessage.Content) : null,
                    LastMessageSender = lastMessage?.SenderDisplayName,
                    LastMessageTime = lastMessage != null ? FormatTime(lastMessage.SentAt) : null,
                    LastMessageIso = lastMessage != null ? lastMessage.SentAt.ToUniversalTime().ToString("o") : null,
                    UnreadCount = unreadCount
                };

                summaries.Add(dto);
            }

            return summaries
                .OrderByDescending(s => s.LastMessageIso ?? string.Empty)
                .ThenBy(s => s.Title)
                .ToList();
        }

        public async Task<ChatSummaryDto?> GetChatSummaryForUserAsync(int chatId, int userId)
        {
            var userChat = await _context.UserChats
                .AsNoTracking()
                .Include(uc => uc.Chat)
                    .ThenInclude(c => c.UserChats)
                        .ThenInclude(uc => uc.User)
                .FirstOrDefaultAsync(uc => uc.ChatId == chatId && uc.UserId == userId && uc.Chat.IsActive);

            if (userChat == null)
                return null;

            var lastMessage = await _context.Messages
                .AsNoTracking()
                .Where(m => m.ChatId == chatId && m.IsActive)
                .OrderByDescending(m => m.SentAt)
                .ThenByDescending(m => m.Id)
                .Select(m => new
                {
                    m.Content,
                    m.SentAt,
                    SenderDisplayName = m.Sender.DisplayName ?? m.Sender.Username,
                    m.Id,
                    m.SenderId
                })
                .FirstOrDefaultAsync();

            var lastReadId = userChat.LastReadMessageId ?? 0;

            var unreadCount = await _context.Messages
                .AsNoTracking()
                .Where(m => m.ChatId == chatId && m.IsActive && m.Id > lastReadId && m.SenderId != userId)
                .CountAsync();

            return new ChatSummaryDto
            {
                ChatId = chatId,
                Title = BuildChatTitle(userChat.Chat, userId),
                IsGroup = userChat.Chat.IsGroup,
                LastMessagePreview = lastMessage != null ? BuildPreview(lastMessage.Content) : null,
                LastMessageSender = lastMessage?.SenderDisplayName,
                LastMessageTime = lastMessage != null ? FormatTime(lastMessage.SentAt) : null,
                LastMessageIso = lastMessage != null ? lastMessage.SentAt.ToUniversalTime().ToString("o") : null,
                UnreadCount = unreadCount
            };
        }

        public async Task<List<MessageDto>> GetMessagesForChatAsync(int chatId)
        {
            var messages = await _context.Messages
                .AsNoTracking()
                .Where(m => m.ChatId == chatId && m.IsActive)
                .Include(m => m.Sender)
                .Include(m => m.ReplyToMessage)
                    .ThenInclude(r => r.Sender)
                .OrderBy(m => m.SentAt)
                .ThenBy(m => m.Id)
                .ToListAsync();

            return messages.Select(MapMessage).ToList();
        }

        public async Task<MessageDto?> GetMessageDtoAsync(int messageId)
        {
            var message = await _context.Messages
                .AsNoTracking()
                .Include(m => m.Sender)
                .Include(m => m.ReplyToMessage)
                    .ThenInclude(r => r.Sender)
                .FirstOrDefaultAsync(m => m.Id == messageId && m.IsActive);

            return message == null ? null : MapMessage(message);
        }

        private static string BuildChatTitle(Chat chat, int userId)
        {
            if (chat.IsGroup)
                return chat.Name;

            var otherUser = chat.UserChats
                .Select(uc => uc.User)
                .FirstOrDefault(u => u.Id != userId);

            return otherUser?.DisplayName ?? otherUser?.Username ?? "Личный чат";
        }

        private static string BuildPreview(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return string.Empty;

            var trimmed = content.Trim();
            return trimmed.Length <= 120 ? trimmed : trimmed.Substring(0, 117) + "…";
        }

        private static string FormatTime(DateTime sentAt)
        {
            return sentAt.ToLocalTime().ToString("HH:mm");
        }

        private static MessageDto MapMessage(Message message)
        {
            return new MessageDto
            {
                Id = message.Id,
                ChatId = message.ChatId,
                Content = message.Content,
                SentAt = FormatTime(message.SentAt),
                SentAtIso = message.SentAt.ToUniversalTime().ToString("o"),
                IsEdited = message.IsEdited,
                Sender = new SenderDto
                {
                    Username = message.Sender.Username,
                    DisplayName = message.Sender.DisplayName ?? message.Sender.Username
                },
                Reply = message.ReplyToMessage != null ? new ReplyDto
                {
                    Id = message.ReplyToMessage.Id,
                    Content = message.ReplyToMessage.Content,
                    SenderDisplayName = message.ReplyToMessage.Sender.DisplayName ?? message.ReplyToMessage.Sender.Username,
                    SenderUsername = message.ReplyToMessage.Sender.Username
                } : null
            };
        }
    }
}
