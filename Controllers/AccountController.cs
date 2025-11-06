using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MessengerApp.Data;
using MessengerApp.Models;
using MessengerApp.Hubs;
using MessengerApp.Services;
using Microsoft.AspNetCore.SignalR;

namespace MessengerApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _chatHubContext;
        private readonly ChatQueryService _chatQueryService;

        public AccountController(AppDbContext context, IHubContext<ChatHub> chatHubContext, ChatQueryService chatQueryService)
        {
            _context = context;
            _chatHubContext = chatHubContext;
            _chatQueryService = chatQueryService;
        }

        // --------------------- РЕГИСТРАЦИЯ ---------------------
        [HttpPost]
        public async Task<IActionResult> Register(string username, string password, string confirmPassword, string displayName, int departmentId)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Пароли не совпадают.";
                ViewBag.Departments = _context.Department.ToList();
                return View("Login");
            }

            if (_context.Users.Any(u => u.Username == username))
            {
                ViewBag.Error = "Такой логин уже существует.";
                ViewBag.Departments = _context.Department.ToList();
                return View("Login");
            }

            var newUser = new User
            {
                Username = username,
                PasswordHash = HashPassword(password),
                DisplayName = displayName,
                DepartmentId = departmentId,
                CreatedAt = DateTime.Now,
                IsActive = true,
                ModerationStatus = "Approved"
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Назначаем роль "User" по умолчанию
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole != null)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = newUser.Id,
                    RoleId = userRole.Id
                });
            }

            // --- Автоматическое подключение к чатам ---
            var globalChat = await _context.Chats.FirstOrDefaultAsync(c => c.Id == 1);
            var department = await _context.Department.FindAsync(departmentId);

            if (globalChat != null)
            {
                _context.UserChats.Add(new UserChat
                {
                    UserId = newUser.Id,
                    ChatId = globalChat.Id,
                    IsAdmin = false
                });
            }

            if (department != null)
            {
                var deptChatName = $"Отдел: {department.Name}";
                var deptChat = await _context.Chats.FirstOrDefaultAsync(c => c.Name == deptChatName);

                if (deptChat != null)
                {
                    _context.UserChats.Add(new UserChat
                    {
                        UserId = newUser.Id,
                        ChatId = deptChat.Id,
                        IsAdmin = false
                    });
                }
            }

            await _context.SaveChangesAsync();

            ViewBag.Message = "Регистрация прошла успешно. Теперь войдите в систему.";
            return RedirectToAction("Login");
        }

        // --------------------- ЛОГИН ---------------------
        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.Departments = _context.Department.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var hashed = HashPassword(password);

            var user = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == hashed);

            if (user == null)
            {
                ViewBag.Error = "Неверный логин или пароль.";
                ViewBag.Departments = _context.Department.ToList();
                return View();
            }

            if (!user.IsActive)
            {
                ViewBag.Error = "Пользователь деактивирован.";
                ViewBag.Departments = _context.Department.ToList();
                return View();
            }

            // Загружаем роли пользователя
            var roles = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("DisplayName", user.DisplayName),
                new Claim("Department", user.Department.Name)
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            // После входа направляем на основную страницу
            return RedirectToAction("Index", "Home");
        }

        // --------------------- ВЫХОД ---------------------
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // --------------------- ОБНОВЛЕНИЕ ПРОФИЛЯ ---------------------
        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return Unauthorized();

            var username = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username))
                return Unauthorized();

            var user = await _context.Users
                .Include(u => u.UserChats)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return NotFound();

            var newDisplayName = request?.DisplayName?.Trim();
            if (string.IsNullOrWhiteSpace(newDisplayName))
                return BadRequest("Имя не может быть пустым");

            user.DisplayName = newDisplayName;

            await _context.SaveChangesAsync();

            // Обновляем клеймы пользователя
            var identity = User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var claim = identity.FindFirst("DisplayName");
                if (claim != null)
                    identity.RemoveClaim(claim);

                identity.AddClaim(new Claim("DisplayName", user.DisplayName));

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity));
            }

            // Обновляем приватные чаты других участников
            var chatIds = user.UserChats.Select(uc => uc.ChatId).ToList();

            foreach (var chatId in chatIds)
            {
                var chat = await _context.Chats
                    .Include(c => c.UserChats)
                        .ThenInclude(uc => uc.User)
                    .FirstOrDefaultAsync(c => c.Id == chatId);

                if (chat == null)
                    continue;

                foreach (var participant in chat.UserChats)
                {
                    var summary = await _chatQueryService.GetChatSummaryForUserAsync(chat.Id, participant.UserId);
                    if (summary != null)
                    {
                        await _chatHubContext.Clients.Group($"user_{participant.User.Username}")
                            .SendAsync("ChatUpdated", summary);
                    }
                }
            }

            await _chatHubContext.Clients.Group($"user_{user.Username}")
                .SendAsync("ProfileUpdated", new { displayName = user.DisplayName });

            return Ok(new { displayName = user.DisplayName });
        }

        // --------------------- УТИЛИТА ---------------------
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }

    public class UpdateProfileRequest
    {
        public string DisplayName { get; set; }
    }
}
