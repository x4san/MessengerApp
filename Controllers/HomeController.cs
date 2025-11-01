using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MessengerApp.Data;
using Microsoft.EntityFrameworkCore;

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
                return RedirectToAction("TestAuth", "Account");

            var username = User.Identity?.Name;
            var user = await _context.Users
                .Include(u => u.UserChats)
                .ThenInclude(uc => uc.Chat)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return RedirectToAction("TestAuth", "Account");

            // Список чатов текущего пользователя
            var chats = user.UserChats
                .Where(uc => uc.Chat.IsActive)
                .Select(uc => uc.Chat)
                .ToList();

            ViewBag.DisplayName = user.DisplayName ?? user.Username;

            return View(chats); // 👈 передаём список чатов в Razor
        }
    }
}
