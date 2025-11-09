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
        options.AccessDeniedPath = "/Account/Login";      // при отсутствии прав
        options.ExpireTimeSpan = TimeSpan.FromDays(1);    // срок жизни cookie
        options.SlidingExpiration = true;                 // обновление при активности

        // ⚙️ API не должно редиректить — отдаём 401/403 напрямую
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                }
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = 403;
                    return Task.CompletedTask;
                }
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            }
        };
    });

// -----------------------------------------------------------------------------
// 🔹 Подключаем MVC (включает Razor + API)
builder.Services.AddControllersWithViews();

// 🔹 Подключаем SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// -----------------------------------------------------------------------------
// 🔹 Настройка пайплайна обработки запросов
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

// 🔑 Подключаем SignalR-хаб
app.MapHub<ChatHub>("/chatHub");

// 🔐 Авторизация и аутентификация
app.UseAuthentication();
app.UseAuthorization();

// -----------------------------------------------------------------------------
// 🔹 Настраиваем маршруты MVC и API

// MVC маршруты
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// API маршруты
app.MapControllers();

// -----------------------------------------------------------------------------
// 🚀 Запуск приложения
app.Run();
