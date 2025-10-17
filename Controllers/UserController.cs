using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MessengerApp.Data;
using MessengerApp.Models;

namespace MessengerApp.Controllers
{
    [Authorize] // только авторизованные пользователи
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // ------------------- СПИСОК ВСЕХ ПОЛЬЗОВАТЕЛЕЙ -------------------
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.Department)
                .OrderBy(u => u.Username)
                .ToListAsync();
            return View(users);
        }

        // ------------------- МОДЕРАЦИЯ -------------------
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Moderate()
        {
            var pendingUsers = await _context.Users
                .Include(u => u.Department)
                .Where(u => u.ModerationStatus == "Pending")
                .ToListAsync();
            return View(pendingUsers);
        }

        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.ModerationStatus = "Approved";
            user.ModeratorComment = null;
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Пользователь {user.Username} одобрен.";
            return RedirectToAction("Moderate");
        }

        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        public async Task<IActionResult> Reject(int id, string comment)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.ModerationStatus = "Rejected";
            user.ModeratorComment = comment;
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Пользователь {user.Username} отклонён.";
            return RedirectToAction("Moderate");
        }

        // ------------------- ДЕАКТИВАЦИЯ (SOFT DELETE) -------------------
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Deactivate(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // 🔒 Проверка: нельзя деактивировать самого себя
            var currentUserName = User.Identity?.Name;
            if (user.Username == currentUserName)
            {
                TempData["Message"] = "Нельзя деактивировать самого себя.";
                return RedirectToAction("Index");
            }

            user.IsActive = false;
            user.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Пользователь {user.Username} деактивирован.";
            return RedirectToAction("Index");
        }

    }
}
