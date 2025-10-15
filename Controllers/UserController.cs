using Microsoft.AspNetCore.Mvc;
using MessengerApp.Data;
using MessengerApp.Models;

namespace MessengerApp.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _db;

        public UserController(AppDbContext db)
        {
            _db = db;
        }

        // 🟩 Список активных пользователей
        public IActionResult Index()
        {
            var users = _db.Users
                .Where(u => u.IsActive)
                .ToList();
            return View(users);
        }

        // 🟧 Мягкое удаление пользователя (деактивация)
        [HttpPost]
        public IActionResult Deactivate(int id)
        {
            var user = _db.Users.Find(id);
            if (user == null)
                return NotFound();

            user.IsActive = false;             // Помечаем как неактивного
            user.DeletedAt = DateTime.Now;     // Записываем время "удаления"
            _db.SaveChanges();                 // Сохраняем изменения

            return RedirectToAction("Index");
        }
    }
}
