using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MessengerApp.Data;
using MessengerApp.Models;

namespace MessengerApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        // Главная страница чатов
        public async Task<IActionResult> Index()
        {
            // Если пользователь не авторизован → редирект на TestAuth
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var username = User.Identity?.Name;

            // Находим пользователя с его чатами и участниками чатов
            var user = await _context.Users
                .Include(u => u.UserChats)
                    .ThenInclude(uc => uc.Chat)
                        .ThenInclude(c => c.UserChats)
                            .ThenInclude(uc => uc.User)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return RedirectToAction("Login", "Account");

            ViewBag.DisplayName = user.DisplayName ?? user.Username;
            ViewBag.Username = user.Username;

            // Формируем список чатов с корректными названиями
            var chatList = user.UserChats
                .Where(uc => uc.Chat.IsActive)
                .Select(uc =>
                {
                    var chat = uc.Chat;

                    if (!chat.IsGroup)
                    {
                        // Ищем собеседника (второго участника)
                        var otherUser = chat.UserChats
                            .Select(x => x.User)
                            .FirstOrDefault(x => x.Id != user.Id);

                        // Подменяем имя чата на DisplayName собеседника
                        chat.Name = otherUser != null ? otherUser.DisplayName : "Личный чат";
                    }

                    return chat;
                })
                // Сортировка: общий чат → групповые → личные
                .OrderBy(c => c.IsGroup ? (c.Name.Contains("Общий") ? 0 : 1) : 2)
                .ThenBy(c => c.Name)
                .ToList();

            return View(chatList);
        }
    }
}
