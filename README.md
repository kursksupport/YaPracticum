Монолит разделён на три независимых ASP.NET Core Web API-сервиса. Каждый сервис имеет собственную PostgreSQL-базу и свою зону ответственности.

Технологии

- C# / .NET 9
- ASP.NET Core Web API
- Entity Framework Core и PostgreSQL
- Apache Kafka (`Confluent.Kafka`)
- JWT-аутентификация и авторизация
- Swagger
- Docker и Docker Compose

Архитектура

Решение находится в папке `Microservices` и состоит из следующих проектов:

- Users — регистрация, вход, хеширование паролей и выдача JWT.
- Events — CRUD событий и учёт доступных мест.
- Bookings — создание, просмотр и отмена броней.
- Contracts — общий контракт Kafka-сообщения BookingConfirmed и имя топика booking-confirmed.

Каждый сервис разделён на слои `Domain`, `Application`, `Infrastructure` и `Api`.

У сервисов нет общих DbContext и навигационных свойств между разными сервисами. В Booking хранятся только EventId и UserId.

Взаимодействие сервисов

Сервисы Events и Bookings не вызывают друг друга по HTTP.

1. Новая бронь создаётся в статусе `Pending`.
2. Фоновый сервис Bookings подтверждает её.
3. После сохранения статуса `Confirmed` Bookings публикует Kafka-сообщение `BookingConfirmed`.
4. Events читает сообщение и уменьшает `AvailableSeats` у соответствующего события.

Система использует eventual consistency: количество мест меняется не в момент HTTP-запроса, а после обработки сообщения Kafka.

Запуск через Docker

Требуется установленный и запущенный Docker Desktop.

Из папки `EventManagementService/Microservices` выполните:

docker compose up --build

Если в системе доступна только старая команда Compose:

docker-compose up --build

Compose поднимает Zookeeper, Kafka, три PostgreSQL-базы и три API. Миграции EF Core применяются автоматически при старте сервисов.

Swagger:

- Users: http://localhost:8081/swagger
- Events: http://localhost:8082/swagger
- Bookings: http://localhost:8083/swagger

Kafka этого Docker-набора доступна с хоста по `localhost:9094`. Внутри Docker-сети сервисы используют `kafka:29092`.

Остановка контейнеров:

docker compose down

Для старой команды: `docker-compose down`.

Чтобы удалить также данные трёх тестовых БД:

docker compose down -v

JWT

JWT выдаёт только Users. Events и Bookings проверяют тот же секрет, издателя и аудиторию.

Для защищённых запросов:

1. Получите токен через `POST /auth/login`.
2. В Swagger Events или Bookings нажмите `Authorize`.
3. Вставьте сам JWT-токен без префикса `Bearer` — Swagger добавит его сам.

API

Users — `http://localhost:8081`

POST /auth/register Регистрация пользователя. Если роль не указана, назначается `User`
POST /auth/login Проверка логина и пароля, возврат JWT-токена

Пример регистрации администратора:

{
  "login": "admin",
  "password": "123456",
  "role": "Admin"
}

Пример создания события:

{
  "title": "Концерт",
  "description": "Проверка Kafka",
  "startAt": "2026-09-01T18:00:00Z",
  "endAt": "2026-09-01T20:00:00Z",
  "totalSeats": 10
}

При создании AvailableSeats равен TotalSeats. После подтверждённой брони он уменьшается на один.

Bookings — http://localhost:8083

Все endpoint требуют JWT.

POST /events/{eventId}/book Создание брони. Возвращает 202 Accepted; новая бронь имеет статус Pending. 
GET /bookings/{id} Получение брони по идентификатору. 
DELETE /bookings/{id} Отмена своей брони; Admin может отменить любую. 

Проверка Kafka-сценария

1. Зарегистрируйте администратора и обычного пользователя в Users.
2. Войдите под администратором и создайте событие в Events. Запомните id и availableSeats.
3. Войдите под обычным пользователем и создайте бронь в Bookings: POST /events/{eventId}/book.
4. Подождите до 5 секунд.
5. Выполните GET /events/{eventId} в Events. availableSeats должен уменьшиться на 1.

Это означает, что Bookings опубликовал BookingConfirmed, а Events обработал его через Kafka.
