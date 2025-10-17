namespace MessengerApp.Models
{
    public class User
    {
        public int Id { get; set; }  // Первичный ключ
        public string Username { get; set; } = string.Empty; // Уникальное имя пользователя
        public string PasswordHash { get; set; } = string.Empty; // Хэш пароля
        public string DisplayName { get; set; } = string.Empty; // Имя, отображаемое в чате
        //department
        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;
        //soft delete
        public bool IsActive { get; set; } = true; // если false — пользователь деактивирован, но не удалён
        public DateTime CreatedAt { get; set; } = DateTime.Now; // дата добавления
        public DateTime? DeletedAt { get; set; } // когда был "удалён" (деактивирован)
        //moderatsiya
        public string ModerationStatus { get; set; } = "Pending"; // Pending / Approved / Rejected
        public string? ModeratorComment { get; set; } // Опционально

        // Навигационные свойства
        //public ICollection<Message>? Messages { get; set; }
        //public ICollection<ChatUser>? ChatUsers { get; set; }
    }
}
