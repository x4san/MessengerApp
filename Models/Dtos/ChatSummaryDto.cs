using System;

namespace MessengerApp.Models.Dtos
{
    public class ChatSummaryDto
    {
        public int ChatId { get; set; }
        public string Title { get; set; }
        public bool IsGroup { get; set; }
        public string? LastMessagePreview { get; set; }
        public string? LastMessageSender { get; set; }
        public string? LastMessageTime { get; set; }
        public string? LastMessageIso { get; set; }
        public int UnreadCount { get; set; }
    }
}
