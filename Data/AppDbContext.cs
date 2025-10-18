using Microsoft.EntityFrameworkCore;
using MessengerApp.Models;
using System.Security.Cryptography;
using System.Text;

namespace MessengerApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Department> Department => Set<Department>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Уникальные логины ---
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // --- Связь User ↔ Department ---
            modelBuilder.Entity<Department>()
                .HasMany(d => d.Users)
                .WithOne(u => u.Department)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Отделы (начальные данные) ---
            modelBuilder.Entity<Department>().HasData(
                new Department { Id = 1, Name = "Терапия" },
                new Department { Id = 2, Name = "Хирургия" },
                new Department { Id = 3, Name = "Лаборатория" },
                new Department { Id = 4, Name = "Рентгенология" },
                new Department { Id = 5, Name = "Регистратура" }
            );

            // --- Роли (начальные данные) ---
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "Moderator" },
                new Role { Id = 3, Name = "User" }
            );

            // --- Связь многие-ко-многим между User и Role ---
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            // --- Локальная функция для хэша пароля ---
            string HashPassword(string password)
            {
                using var sha = SHA256.Create();
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }

            // --- Пользователи (начальные данные) ---
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = HashPassword("123"),
                    DisplayName = "Администратор",
                    DepartmentId = 1,
                    IsActive = true,
                    ModerationStatus = "Approved",
                    CreatedAt = new DateTime(2025, 1, 1)
                },
                new User
                {
                    Id = 2,
                    Username = "mod",
                    PasswordHash = HashPassword("123"),
                    DisplayName = "Модератор",
                    DepartmentId = 1,
                    IsActive = true,
                    ModerationStatus = "Approved",
                    CreatedAt = new DateTime(2025, 1, 1)
                },
                new User
                {
                    Id = 3,
                    Username = "user",
                    PasswordHash = HashPassword("123"),
                    DisplayName = "Пользователь",
                    DepartmentId = 1,
                    IsActive = true,
                    ModerationStatus = "Approved",
                    CreatedAt = new DateTime(2025, 1, 1)
                }
            );
            
            // --- Связи пользователей с ролями ---
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { UserId = 1, RoleId = 1 }, // admin → Admin
                new UserRole { UserId = 2, RoleId = 2 }, // mod → Moderator
                new UserRole { UserId = 3, RoleId = 3 }  // user → User
            );
        }
    }
}
