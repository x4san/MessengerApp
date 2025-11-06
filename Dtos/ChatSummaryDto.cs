namespace MessengerApp.Dtos
{
    public class ChatSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? LastMessage { get; set; }
        public string? LastSender { get; set; }
        public DateTime? LastMessageUtc { get; set; }
        public int? LastMessageId { get; set; }
        public int UnreadCount { get; set; }
        public bool IsGroup { get; set; }
        public string AvatarInitials { get; set; } = string.Empty;
        public string AvatarColor { get; set; } = string.Empty;
    }
}
