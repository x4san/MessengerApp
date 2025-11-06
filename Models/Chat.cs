using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MessengerApp.Models
{
    public class Chat
    {
        [Key]
        public int Id { get; set; }

        // 🏷 Название чата (для групп и каналов)
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // 💬 Признак группового чата
        public bool IsGroup { get; set; } = false;

        // 🔒 Признак приватного чата (например, только админ пишет)
        public bool IsPrivate { get; set; } = false;

        // 📅 Дата создания
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 🧩 Soft delete
        public bool IsActive { get; set; } = true;
        public DateTime? DeletedAt { get; set; }

        // 🔗 Связи
        public ICollection<UserChat> UserChats { get; set; } = new List<UserChat>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
