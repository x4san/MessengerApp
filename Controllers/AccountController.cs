using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MessengerApp.Data;
using MessengerApp.Models;

namespace MessengerApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // --------------------- РЕГИСТРАЦИЯ ---------------------
        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Departments = _context.Department.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string password, string confirmPassword, string displayName, int departmentId)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Пароли не совпадают.";
                ViewBag.Departments = _context.Department.ToList();
                return View();
            }

            if (_context.Users.Any(u => u.Username == username))
            {
                ViewBag.Error = "Такой логин уже существует.";
                ViewBag.Departments = _context.Department.ToList();
                return View();
            }

            var newUser = new User
            {
                Username = username,
                PasswordHash = HashPassword(password),
                DisplayName = displayName,
                DepartmentId = departmentId,
                CreatedAt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                         DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second),
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
                await _context.SaveChangesAsync();
            }

            ViewBag.Message = "Регистрация прошла успешно. Теперь войдите в систему.";


            //------------------------------------------------------------------------------------###
            //return RedirectToAction("Login");
            return View();
            //------------------------------------------------------------------------------------###

        }

        // --------------------- ЛОГИН ---------------------
        [HttpGet]
        public IActionResult Login() => View();

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
                return View();
            }

            if (!user.IsActive)
            {
                ViewBag.Error = "Пользователь деактивирован.";
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



            //------------------------------------------------------------------------------------###
            return RedirectToAction("Index", "Home");
            //return View();
            //------------------------------------------------------------------------------------###


        }

        // --------------------- ВЫХОД ---------------------
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // После выхода сразу перебрасываем на логин или тестовую страницу
            return RedirectToAction("TestAuth"); // 👈 во время тестов
            // return RedirectToAction("Login");  // 👈 в “боевой” версии
        }


        // --------------------- УТИЛИТА ---------------------
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
        // 🔰 DEBUG: тестовая страница авторизации (удалить после проверки)
        [HttpGet]
        public IActionResult TestAuth()
        {
            ViewBag.Departments = _context.Department.ToList();
            return View();
        }
        // 🔰 END DEBUG

    }
}
