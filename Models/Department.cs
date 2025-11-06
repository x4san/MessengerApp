namespace MessengerApp.Models
{
    public class Department
    {
        public int Id { get; set; }  // Первичный ключ
        public string Name { get; set; } = string.Empty;  // Название отдела

        // Навигация
        public ICollection<User>? Users { get; set; }
    }
}
