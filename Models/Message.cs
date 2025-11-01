using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MessengerApp.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ChatId { get; set; }
        [ForeignKey(nameof(ChatId))]
        public Chat Chat { get; set; }

        [Required]
        public int SenderId { get; set; }
        [ForeignKey(nameof(SenderId))]
        public User Sender { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // ✏️ Редактирование и удаление
        public bool IsEdited { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime? DeletedAt { get; set; }

        // 🧩 Ответы (reply)
        public int? ReplyToMessageId { get; set; }
        [ForeignKey(nameof(ReplyToMessageId))]
        public Message? ReplyToMessage { get; set; }
    }
}
