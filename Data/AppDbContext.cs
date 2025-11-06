using Microsoft.EntityFrameworkCore;
using MessengerApp.Models;
using Microsoft.EntityFrameworkCore.Diagnostics;

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
        public DbSet<Chat> Chats => Set<Chat>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<UserChat> UserChats => Set<UserChat>();

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

            // --- Отделы ---
            modelBuilder.Entity<Department>().HasData(
                new Department { Id = 1, Name = "Терапия" },
                new Department { Id = 2, Name = "Хирургия" },
                new Department { Id = 3, Name = "Лаборатория" },
                new Department { Id = 4, Name = "Рентгенология" },
                new Department { Id = 5, Name = "Регистратура" }
            );

            // --- Роли ---
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "Moderator" },
                new Role { Id = 3, Name = "User" }
            );

            // --- Связь многие-ко-многим между User и Role ---
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            // --- Связь UserChat ---
            modelBuilder.Entity<UserChat>()
                .HasKey(uc => new { uc.UserId, uc.ChatId });

            modelBuilder.Entity<UserChat>()
                .HasOne(uc => uc.User)
                .WithMany(u => u.UserChats)
                .HasForeignKey(uc => uc.UserId);

            modelBuilder.Entity<UserChat>()
                .HasOne(uc => uc.Chat)
                .WithMany(c => c.UserChats)
                .HasForeignKey(uc => uc.ChatId);

            // --- Связь Message ---
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Chat)
                .WithMany(c => c.Messages)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            // --- Статические значения ---
            const string hash123 = "a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3"; // SHA256("123")
            var fixedDate = new DateTime(2025, 1, 1, 0, 0, 0);

            // --- Пользователи ---
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = hash123,
                    DisplayName = "Администратор",
                    Bio = "Руководитель чатов",
                    DepartmentId = 1,
                    IsActive = true,
                    ModerationStatus = "Approved",
                    CreatedAt = fixedDate
                },
                new User
                {
                    Id = 2,
                    Username = "mod",
                    PasswordHash = hash123,
                    DisplayName = "Модератор",
                    Bio = "Следит за порядком",
                    DepartmentId = 2,
                    IsActive = true,
                    ModerationStatus = "Approved",
                    CreatedAt = fixedDate
                },
                new User
                {
                    Id = 3,
                    Username = "user",
                    PasswordHash = hash123,
                    DisplayName = "Пользователь",
                    Bio = "Всегда на связи",
                    DepartmentId = 3,
                    IsActive = true,
                    ModerationStatus = "Approved",
                    CreatedAt = fixedDate
                }
            );

            // --- Роли пользователей ---
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { UserId = 1, RoleId = 1 },
                new UserRole { UserId = 2, RoleId = 2 },
                new UserRole { UserId = 3, RoleId = 3 }
            );

            // --- Чаты ---
            modelBuilder.Entity<Chat>().HasData(
                // 1. Общий чат всех сотрудников
                new Chat
                {
                    Id = 1,
                    Name = "Общий чат всех сотрудников",
                    IsGroup = true,
                    IsPrivate = false,
                    IsActive = true,
                    CreatedAt = fixedDate
                },

                // 2–6. Чаты отделов
                new Chat { Id = 2, Name = "Отдел: Терапия", IsGroup = true, IsPrivate = false, IsActive = true, CreatedAt = fixedDate },
                new Chat { Id = 3, Name = "Отдел: Хирургия", IsGroup = true, IsPrivate = false, IsActive = true, CreatedAt = fixedDate },
                new Chat { Id = 4, Name = "Отдел: Лаборатория", IsGroup = true, IsPrivate = false, IsActive = true, CreatedAt = fixedDate },
                new Chat { Id = 5, Name = "Отдел: Рентгенология", IsGroup = true, IsPrivate = false, IsActive = true, CreatedAt = fixedDate },
                new Chat { Id = 6, Name = "Отдел: Регистратура", IsGroup = true, IsPrivate = false, IsActive = true, CreatedAt = fixedDate },

                // 7–9. Личные чаты
                new Chat { Id = 7, Name = "ЛС: Админ ↔ Модератор", IsGroup = false, IsPrivate = true, IsActive = true, CreatedAt = fixedDate },
                new Chat { Id = 8, Name = "ЛС: Админ ↔ Пользователь", IsGroup = false, IsPrivate = true, IsActive = true, CreatedAt = fixedDate },
                new Chat { Id = 9, Name = "ЛС: Модератор ↔ Пользователь", IsGroup = false, IsPrivate = true, IsActive = true, CreatedAt = fixedDate }
            );

            // --- Привязки пользователей к чатам ---
            modelBuilder.Entity<UserChat>().HasData(
                // Общий
                new UserChat { UserId = 1, ChatId = 1, IsAdmin = true, LastReadAt = fixedDate },
                new UserChat { UserId = 2, ChatId = 1, LastReadAt = fixedDate },
                new UserChat { UserId = 3, ChatId = 1, LastReadAt = fixedDate },

                // Отделы
                new UserChat { UserId = 1, ChatId = 2, LastReadAt = fixedDate }, // админ — терапия
                new UserChat { UserId = 2, ChatId = 3, LastReadAt = fixedDate }, // мод — хирургия
                new UserChat { UserId = 3, ChatId = 4, LastReadAt = fixedDate }, // юзер — лаборатория

                // ЛС
                new UserChat { UserId = 1, ChatId = 7, LastReadAt = fixedDate },
                new UserChat { UserId = 2, ChatId = 7, LastReadAt = fixedDate },
                new UserChat { UserId = 1, ChatId = 8, LastReadAt = fixedDate },
                new UserChat { UserId = 3, ChatId = 8, LastReadAt = fixedDate },
                new UserChat { UserId = 2, ChatId = 9, LastReadAt = fixedDate },
                new UserChat { UserId = 3, ChatId = 9, LastReadAt = fixedDate }
            );
        }

        // --- Отключаем предупреждение EF о “динамической модели” ---
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(w =>
                w.Ignore(RelationalEventId.PendingModelChangesWarning));
        }
    }
}
