namespace MessengerApp.Dtos
{
    public class MessageDto
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public string Sender { get; set; } = string.Empty;
        public int SenderId { get; set; }
        public string SenderUsername { get; set; } = string.Empty;
        public string AvatarInitials { get; set; } = string.Empty;
        public string AvatarColor { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime SentAtUtc { get; set; }
        public string Time { get; set; } = string.Empty;
        public bool IsEdited { get; set; }
        public MessageReplyDto? Reply { get; set; }
    }

    public class MessageReplyDto
    {
        public int Id { get; set; }
        public string Sender { get; set; } = string.Empty;
        public string Snippet { get; set; } = string.Empty;
    }
}
