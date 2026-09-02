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

Compose поднимает Zookeeper, Kafka, три PostgreSQL-базы, три API, Redis и стек наблюдаемости. Миграции EF Core применяются автоматически при старте сервисов.

Swagger:

- Users: http://localhost:8081/swagger
- Events: http://localhost:8082/swagger
- Bookings: http://localhost:8083/swagger

Kafka этого Docker-набора доступна с хоста по `localhost:9094`. Внутри Docker-сети сервисы используют `kafka:29092`.

Наблюдаемость

Во все три API-сервиса интегрирован OpenTelemetry SDK. Автоматически собираются:

- трейсы входящих ASP.NET Core-запросов, исходящих HTTP-запросов и запросов Entity Framework Core;
- метрики ASP.NET Core, включая latency, throughput, error rate и количество активных запросов;
- метрики рантайма .NET, включая GC, память, CPU, JIT и thread pool.

Трейсы экспортируются по OTLP gRPC в Jaeger. Метрики каждого сервиса публикуются в формате Prometheus на эндпоинте `/metrics`. Логи выводятся через Serilog в компактном JSON-формате, по одному JSON-объекту на строку.

В стек наблюдаемости входят:

- Prometheus — хранение и запрос метрик;
- Jaeger — хранение и просмотр распределённых трейсов;
- Grafana — визуализация latency, throughput, active requests и error rate;
- OpenTelemetry — сбор и экспорт телеметрии из сервисов;
- Serilog — структурированные JSON-логи.

Порты и адреса:

- Prometheus UI и Targets: http://localhost:9090, http://localhost:9090/targets
- Jaeger UI: http://localhost:16686
- Jaeger OTLP gRPC endpoint: `http://localhost:4317`
- Grafana UI: http://localhost:3000
- Users metrics: http://localhost:8081/metrics
- Events metrics: http://localhost:8082/metrics
- Bookings metrics: http://localhost:8083/metrics

Запуск всего приложения вместе со стеком мониторинга:

cd EventManagementService/Microservices
docker compose up --build -d

Проверить состояние контейнеров:

docker compose ps

Prometheus использует конфигурацию `EventManagementService/Microservices/prometheus.yml` и скрейпит все три API каждые 15 секунд. На странице Prometheus Targets сервисы `users-service`, `events-service` и `bookings-service` должны иметь статус `UP`.

Для первого входа в Grafana используйте логин `admin` и пароль `admin`. В новой Grafana добавьте источник данных типа Prometheus с адресом `http://prometheus:9090`, затем импортируйте дашборд из файла `EventManagementService/Microservices/grafana-dashboard.json`. Дашборд `Event API Observability` содержит панели latency p50/p95/p99, throughput, active requests и HTTP error rate для всех трёх сервисов.


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

Стратегия кеширования

В сервисе используется паттерн Cache-Aside. При запросе события по идентификатору сервис сначала ищет данные в Redis по ключу `event:{id}`. Если ключа нет, событие загружается из базы данных и сохраняется в кеш. Время жизни такого ключа — 10 минут.
Список десяти самых популярных событий хранится по ключу `events:top10`. Для него установлен TTL 5 минут. Этот список является рейтинговым агрегатом, поэтому небольшое устаревание допустимо. Кеш топа не удаляется после каждого изменения или бронирования, чтобы не создавать лишние обращения к Redis.
Для отдельного события выбрана инвалидация при записи. После обновления или удаления события сначала сохраняются изменения в базе данных, а затем удаляется ключ `event:{id}`. Следующий запрос получит актуальные данные из базы и снова заполнит кеш. При создании события удалять индивидуальный ключ не требуется, потому что событие получает новый идентификатор и старого значения с таким ключом нет.
Kafka-обработчик уменьшает количество доступных мест через тот же сервис событий. После сохранения нового количества мест ключ события также удаляется.
Если Redis недоступен, ошибка записывается в лог, но не передаётся клиенту. Чтение в таком случае считается промахом кеша, и данные загружаются напрямую из базы.

