Первый спринт Яндекс Практикум.
Сервис для управления мероприятиями на ASP.NET Core Web API.

Используемые технологии:
C#
.NET 9
ASP.NET Core Web API
Swagger

эндпоинты REST API:
GET /events — получить список всех событий;
GET /events/{id} — получить событие по id;
POST /events — создать событие;
PUT /events/{id} — обновить событие целиком; 
DELETE /events/{id} — удалить событие;

Валидация
обязательность поля Title;
обязательность StartAt и EndAt;
EndAt должно быть позже StartAt.

Как запустить проект:
1. Клонировать репозиторий
  git clone https://github.com/kursksupport/YaPracticum
2. Перейти в папку проекта
3. Запустить проект
  dotnet run
После запуска в консоли появятся адреса приложения.
