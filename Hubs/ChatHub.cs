using Microsoft.AspNetCore.SignalR;
using MessengerApp.Data;
using MessengerApp.Models;
using MessengerApp.Services;
using Microsoft.EntityFrameworkCore;

namespace MessengerApp.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;
        private readonly ChatPresentationService _presentationService;

        public ChatHub(AppDbContext context, ChatPresentationService presentationService)
        {
            _context = context;
            _presentationService = presentationService;
        }

        public override async Task OnConnectedAsync()
        {
            if (Context.User?.Identity?.IsAuthenticated == true)
            {
                var user = await GetCurrentUserAsync();
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{user.Id}");
            }

            await base.OnConnectedAsync();
        }

        public async Task SubscribeChatList()
        {
            var user = await GetCurrentUserAsync();
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{user.Id}");
            var chats = await _presentationService.GetChatSummariesAsync(user.Id);
            await Clients.Caller.SendAsync("ChatsSnapshot", chats);
        }

        public async Task JoinChat(int chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatId}");

            var user = await GetCurrentUserAsync();
            var membership = await _context.UserChats.FirstOrDefaultAsync(uc => uc.ChatId == chatId && uc.UserId == user.Id);
            if (membership == null)
            {
                throw new HubException("Чат недоступен");
            }

            membership.LastReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var summary = await _presentationService.GetChatSummaryAsync(chatId, user.Id);
            if (summary != null)
            {
                await Clients.Group($"user_{user.Id}").SendAsync("ChatUpdated", summary);
            }
        }

        public async Task MarkChatRead(int chatId)
        {
            var user = await GetCurrentUserAsync();
            var membership = await _context.UserChats.FirstOrDefaultAsync(uc => uc.ChatId == chatId && uc.UserId == user.Id);
            if (membership == null)
            {
                return;
            }

            membership.LastReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var summary = await _presentationService.GetChatSummaryAsync(chatId, user.Id);
            if (summary != null)
            {
                await Clients.Group($"user_{user.Id}").SendAsync("ChatUpdated", summary);
            }
        }

        public async Task SendMessage(int chatId, string content, int? replyToMessageId)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return;
            }

            var user = await GetCurrentUserAsync();
            var chat = await _context.Chats
                .Include(c => c.UserChats)
                .FirstOrDefaultAsync(c => c.Id == chatId && c.IsActive);

            if (chat == null)
            {
                throw new HubException("Чат не найден");
            }

            var replyTo = replyToMessageId.HasValue
                ? await _context.Messages.FirstOrDefaultAsync(m => m.Id == replyToMessageId.Value && m.ChatId == chatId && m.IsActive)
                : null;

            var message = new Message
            {
                ChatId = chatId,
                SenderId = user.Id,
                Content = content.Trim(),
                SentAt = DateTime.UtcNow,
                ReplyToMessageId = replyTo?.Id
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var membership = await _context.UserChats.FirstOrDefaultAsync(uc => uc.ChatId == chatId && uc.UserId == user.Id);
            if (membership != null)
            {
                membership.LastReadAt = message.SentAt;
                await _context.SaveChangesAsync();
            }

            var dto = await _presentationService.GetMessageAsync(message.Id);
            if (dto != null)
            {
                await Clients.Group($"chat_{chatId}").SendAsync("ReceiveMessage", dto);
            }

            await BroadcastChatUpdates(chatId, chat.UserChats.Select(uc => uc.UserId));
        }

        public async Task EditMessage(int messageId, string newContent)
        {
            if (string.IsNullOrWhiteSpace(newContent))
            {
                return;
            }

            var user = await GetCurrentUserAsync();
            var message = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Chat)
                    .ThenInclude(c => c.UserChats)
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null || message.SenderId != user.Id)
            {
                throw new HubException("Вы можете редактировать только свои сообщения");
            }

            message.Content = newContent.Trim();
            message.IsEdited = true;
            await _context.SaveChangesAsync();

            var dto = await _presentationService.GetMessageAsync(message.Id);
            if (dto != null)
            {
                await Clients.Group($"chat_{message.ChatId}").SendAsync("MessageEdited", dto);
            }

            var participantIds = message.Chat.UserChats.Select(uc => uc.UserId);
            await BroadcastChatUpdates(message.ChatId, participantIds);
        }

        private async Task BroadcastChatUpdates(int chatId, IEnumerable<int> participantIds)
        {
            foreach (var participantId in participantIds)
            {
                var summary = await _presentationService.GetChatSummaryAsync(chatId, participantId);
                if (summary != null)
                {
                    await Clients.Group($"user_{participantId}").SendAsync("ChatUpdated", summary);
                }
            }
        }

        private async Task<User> GetCurrentUserAsync()
        {
            var username = Context.User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new HubException("Необходима авторизация");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
            if (user == null)
            {
                throw new HubException("Пользователь не найден");
            }

            return user;
        }
    }
}
