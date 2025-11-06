using MessengerApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using MessengerApp.Hubs;


var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// 🔹 Подключаем базу данных SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=messenger.db"));

// -----------------------------------------------------------------------------
// 🔹 Подключаем аутентификацию через Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";             // страница логина
        options.LogoutPath = "/Account/Logout";           // страница выхода
        options.AccessDeniedPath = "/Account/Login"; // если нет прав
        options.ExpireTimeSpan = TimeSpan.FromDays(1);    // срок жизни cookie
        options.SlidingExpiration = true;                 // обновлять при активности
    });

// -----------------------------------------------------------------------------
// 🔹 Подключаем MVC
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
var app = builder.Build();

// -----------------------------------------------------------------------------
// 🔹 Настраиваем пайплайн
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.MapHub<ChatHub>("/chatHub");


// 🔑 Важно: порядок имеет значение
app.UseAuthentication();   // обязательно ДО Authorization
app.UseAuthorization();

// -----------------------------------------------------------------------------
// 🔹 Настраиваем маршруты
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// -----------------------------------------------------------------------------

app.Run();
