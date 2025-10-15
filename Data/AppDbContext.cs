using Microsoft.EntityFrameworkCore;
using MessengerApp.Models;

namespace MessengerApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Уникальные логины
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // ❌ УБЕДИСЬ, что ничего связанного с ChatUser здесь нет!
            // modelBuilder.Entity<ChatUser>().HasKey(cu => new { cu.ChatId, cu.UserId });
        }
    }
}
