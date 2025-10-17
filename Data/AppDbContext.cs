using Microsoft.EntityFrameworkCore;
using MessengerApp.Models;

namespace MessengerApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Department> Departments => Set<Department>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Уникальные логины
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // ❌ УБЕДИСЬ, что ничего связанного с ChatUser здесь нет!
            // modelBuilder.Entity<ChatUser>().HasKey(cu => new { cu.ChatId, cu.UserId });

            modelBuilder.Entity<Department>()
                    .HasMany(d => d.Users)
                    .WithOne(u => u.Department)
                    .HasForeignKey(u => u.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Department>().HasData(
                new Department { Id = 1, Name = "Терапия" },
                new Department { Id = 2, Name = "Хирургия" },
                new Department { Id = 3, Name = "Лаборатория" },
                new Department { Id = 4, Name = "Рентгенология" },
                new Department { Id = 5, Name = "Регистратура" }
            );

        }
    }
}
