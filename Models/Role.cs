namespace MessengerApp.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Навигация
        public ICollection<UserRole>? UserRoles { get; set; }
    }
}
