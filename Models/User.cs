namespace MessengerApp.Models
{
    public class User
    {
        public int Id { get; set; }  // Первичный ключ

        public string Username { get; set; } = string.Empty; // Уникальное имя пользователя
        public string PasswordHash { get; set; } = string.Empty; // Хэш пароля
        public string DisplayName { get; set; } = string.Empty; // Имя, отображаемое в чате
        // 🔹 Мягкое удаление (soft delete)
        public bool IsActive { get; set; } = true; // если false — пользователь деактивирован, но не удалён
        public DateTime CreatedAt { get; set; } = DateTime.Now; // дата добавления
        public DateTime? DeletedAt { get; set; } // когда был "удалён" (деактивирован)

        // Навигационные свойства
        //public ICollection<Message>? Messages { get; set; }
        //public ICollection<ChatUser>? ChatUsers { get; set; }
    }
}
