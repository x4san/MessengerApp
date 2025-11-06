using System;

namespace MessengerApp.Models.Dtos
{
    public class MessageDto
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public string Content { get; set; }
        public string SentAt { get; set; }
        public string SentAtIso { get; set; }
        public bool IsEdited { get; set; }
        public SenderDto Sender { get; set; }
        public ReplyDto? Reply { get; set; }
    }

    public class SenderDto
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
    }

    public class ReplyDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string SenderDisplayName { get; set; }
        public string SenderUsername { get; set; }
    }
}
