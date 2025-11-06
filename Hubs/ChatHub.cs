using Microsoft.AspNetCore.SignalR;
using MessengerApp.Data;
using MessengerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace MessengerApp.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        // Подключение к конкретному чату
        public async Task JoinChat(int chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatId}");
        }

        // Отправка сообщения в чат
        public async Task SendMessage(int chatId, string senderUsername, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return;

            var sender = await _context.Users.FirstOrDefaultAsync(u => u.Username == senderUsername);
            if (sender == null)
                return;

            // Создаём сообщение
            var message = new Message
            {
                ChatId = chatId,
                SenderId = sender.Id,
                Content = content,
                SentAt = DateTime.Now
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Рассылаем всем участникам в SignalR-группе
            await Clients.Group($"chat_{chatId}")
                .SendAsync("ReceiveMessage",
                    sender.DisplayName ?? sender.Username,
                    content,
                    message.SentAt.ToString("HH:mm"));
        }
    }
}
