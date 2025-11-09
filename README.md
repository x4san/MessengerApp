Модель User и БД	/Models/User.cs, /Data/AppDbContext.cs

поля id юзнейм дисплнейм парольхэш отдел времясоздания включен? времяудаления статусмодерации комментмодерации

Регистрация / логин	/Controllers/AccountController.cs, /Views/Account/* sha256 coockie

сделана

Модели Chat, Message	/Models/Chat.cs, /Models/Message.cs

сделаны

SignalR чат

тоже

http://localhost:5000/api/*
http://localhost:5000/account/login
http://localhost:5000/home/index
http://localhost:5000/admin/sqlconsole


dotnet ef migrations add [название]
dotnet ef database update

git push origin [имя ветки]
