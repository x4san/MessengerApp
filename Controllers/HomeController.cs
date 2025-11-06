using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MessengerApp.Data;
using MessengerApp.Models;
using MessengerApp.Services;

namespace MessengerApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ChatPresentationService _presentationService;

        public HomeController(AppDbContext context, ChatPresentationService presentationService)
        {
            _context = context;
            _presentationService = presentationService;
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
            ViewBag.Bio = user.Bio;

            var summaries = await _presentationService.GetChatSummariesAsync(user.Id);
            ViewBag.InitialChats = summaries.Select(c => new
            {
                id = c.Id,
                name = c.Name,
                lastMessage = c.LastMessage,
                lastSender = c.LastSender,
                lastMessageTime = c.LastMessageUtc?.ToLocalTime().ToString("HH:mm"),
                lastMessageUtc = c.LastMessageUtc,
                lastMessageId = c.LastMessageId,
                unreadCount = c.UnreadCount,
                isGroup = c.IsGroup,
                avatarInitials = c.AvatarInitials,
                avatarColor = c.AvatarColor
            }).ToList();

            return View();
        }
    }
}
