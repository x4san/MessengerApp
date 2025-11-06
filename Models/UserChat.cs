using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MessengerApp.Models
{
    public class UserChat
    {
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        public int ChatId { get; set; }
        [ForeignKey(nameof(ChatId))]
        public Chat Chat { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // 👑 Админ / модератор
        public bool IsAdmin { get; set; } = false;

        // 📬 Прочитанные сообщения
        public int? LastReadMessageId { get; set; }
        public DateTime? LastReadAt { get; set; }
    }
}
