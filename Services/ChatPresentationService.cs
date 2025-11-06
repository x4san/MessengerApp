using MessengerApp.Data;
using MessengerApp.Dtos;
using MessengerApp.Utils;
using Microsoft.EntityFrameworkCore;

namespace MessengerApp.Services
{
    public class ChatPresentationService
    {
        private readonly AppDbContext _context;

        public ChatPresentationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ChatSummaryDto>> GetChatSummariesAsync(int userId)
        {
            var userChats = await _context.UserChats
                .Where(uc => uc.UserId == userId && uc.Chat.IsActive)
                .Include(uc => uc.Chat)
                    .ThenInclude(c => c.UserChats)
                        .ThenInclude(uc => uc.User)
                .Include(uc => uc.Chat)
                    .ThenInclude(c => c.Messages)
                        .ThenInclude(m => m.Sender)
                .AsNoTracking()
                .ToListAsync();

            var summaries = new List<ChatSummaryDto>();

            foreach (var membership in userChats)
            {
                summaries.Add(BuildChatSummary(membership, membership.Chat.Messages, userId));
            }

            return summaries
                .OrderByDescending(c => c.LastMessageUtc ?? DateTime.MinValue)
                .ThenBy(c => c.Name)
                .ToList();
        }

        public async Task<ChatSummaryDto?> GetChatSummaryAsync(int chatId, int userId)
        {
            var membership = await _context.UserChats
                .Where(uc => uc.ChatId == chatId && uc.UserId == userId)
                .Include(uc => uc.Chat)
                    .ThenInclude(c => c.UserChats)
                        .ThenInclude(uc => uc.User)
                .Include(uc => uc.Chat)
                    .ThenInclude(c => c.Messages)
                        .ThenInclude(m => m.Sender)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (membership == null)
            {
                return null;
            }

            return BuildChatSummary(membership, membership.Chat.Messages, userId);
        }

        public async Task<List<MessageDto>> GetMessagesAsync(int chatId)
        {
            var messages = await _context.Messages
                .Where(m => m.ChatId == chatId && m.IsActive)
                .Include(m => m.Sender)
                .Include(m => m.ReplyToMessage)
                    .ThenInclude(r => r.Sender)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            return messages.Select(ToDto).ToList();
        }

        public async Task<MessageDto?> GetMessageAsync(int messageId)
        {
            var message = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.ReplyToMessage)
                    .ThenInclude(r => r.Sender)
                .FirstOrDefaultAsync(m => m.Id == messageId);

            return message == null ? null : ToDto(message);
        }

        private static ChatSummaryDto BuildChatSummary(Models.UserChat membership, IEnumerable<Models.Message> messages, int viewerId)
        {
            var chat = membership.Chat;
            var displayName = chat.IsGroup
                ? chat.Name
                : chat.UserChats.FirstOrDefault(uc => uc.UserId != viewerId)?.User.DisplayName ?? chat.Name;

            var lastMessage = messages
                .Where(m => m.IsActive)
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefault();

            var summary = new ChatSummaryDto
            {
                Id = chat.Id,
                Name = displayName,
                IsGroup = chat.IsGroup,
                AvatarInitials = ChatFormattingHelper.BuildInitials(displayName),
                AvatarColor = ChatFormattingHelper.PickColor(displayName + chat.Id)
            };

            if (lastMessage != null)
            {
                summary.LastMessage = lastMessage.Content;
                summary.LastSender = lastMessage.Sender?.DisplayName ?? lastMessage.Sender?.Username;
                summary.LastMessageUtc = lastMessage.SentAt;
                summary.LastMessageId = lastMessage.Id;
            }

            var lastRead = membership.LastReadAt ?? membership.JoinedAt;
            summary.UnreadCount = messages
                .Where(m => m.IsActive && m.SenderId != viewerId && m.SentAt > lastRead)
                .Count();

            return summary;
        }

        private static MessageDto ToDto(Models.Message message)
        {
            var senderName = message.Sender?.DisplayName ?? message.Sender?.Username ?? "";
            var dto = new MessageDto
            {
                Id = message.Id,
                ChatId = message.ChatId,
                Sender = senderName,
                SenderId = message.SenderId,
                SenderUsername = message.Sender?.Username ?? string.Empty,
                AvatarInitials = ChatFormattingHelper.BuildInitials(senderName),
                AvatarColor = ChatFormattingHelper.PickColor(message.Sender?.Username ?? senderName),
                Content = message.Content,
                SentAtUtc = message.SentAt,
                Time = message.SentAt.ToLocalTime().ToString("HH:mm"),
                IsEdited = message.IsEdited
            };

            if (message.ReplyToMessage != null)
            {
                var replySender = message.ReplyToMessage.Sender?.DisplayName ?? message.ReplyToMessage.Sender?.Username ?? "";
                dto.Reply = new MessageReplyDto
                {
                    Id = message.ReplyToMessage.Id,
                    Sender = replySender,
                    Snippet = ChatFormattingHelper.BuildReplySnippet(message.ReplyToMessage.Content)
                };
            }

            return dto;
        }
    }
}
