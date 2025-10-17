using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MessengerApp.Data;
using MessengerApp.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;


namespace MessengerApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // --------------------- LOGIN ---------------------
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Введите логин и пароль.";
                return View();
            }

            var hashed = HashPassword(password);
            var user = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == hashed);

            if (user == null)
            {
                ViewBag.Error = "Неверный логин или пароль.";
                return View();
            }

            if (!user.IsActive)
            {
                ViewBag.Error = "Пользователь деактивирован.";
                return View();
            }

            if (user.ModerationStatus == "Pending")
            {
                ViewBag.Error = "Аккаунт ожидает подтверждения модератором.";
                return View();
            }

            if (user.ModerationStatus == "Rejected")
            {
                ViewBag.Error = $"Регистрация отклонена: {user.ModeratorComment}";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("DisplayName", user.DisplayName),
                new Claim("Department", user.Department.Name)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        // --------------------- REGISTER ---------------------
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            ViewBag.Departments = _context.Departments.ToList();
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(string username, string password, string confirmPassword, string displayName, int departmentId)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Пароли не совпадают.";
                ViewBag.Departments = _context.Departments.ToList();
                return View();
            }

            if (_context.Users.Any(u => u.Username == username))
            {
                ViewBag.Error = "Такой логин уже существует.";
                ViewBag.Departments = _context.Departments.ToList();
                return View();
            }

            var newUser = new User
            {
                Username = username,
                PasswordHash = HashPassword(password),
                DisplayName = displayName,
                DepartmentId = departmentId,
                CreatedAt = DateTime.Now,
                ModerationStatus = "Pending",
                IsActive = true
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            ViewBag.Message = "Регистрация прошла успешно. Ожидайте подтверждения модератором.";
            return RedirectToAction("Login");
        }

        // --------------------- LOGOUT ---------------------
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // --------------------- ACCESS DENIED ---------------------
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // --------------------- UTILS ---------------------
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}
