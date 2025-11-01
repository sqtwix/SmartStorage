# 🏗 SmartStorage Backend

## 📖 Описание проекта

**SmartStorage** — интеллектуальная система управления складом.
Она объединяет **роботов-сканеров**, **backend**, **AI-сервис** и **фронтенд-панель** для автоматизации учёта товаров, мониторинга и предсказания запасов.

Архитектура проекта:

* ⚙️ **SmartStorageBackend** — основной сервер на ASP.NET Core + PostgreSQL
* 🤖 **Robot Emulator** — Python-скрипт, эмулирующий роботов на складе
* 🧩 **PyRobot Wrapper** — прослойка между AI-модулем и системой
* 🧠 **AI Service** — Python-модуль с логикой прогнозирования (вызовы из обёртки)

---

## 🚀 Быстрый старт

### 1️⃣ Установка зависимостей

#### Backend:

```bash
cd SmartStorageBackend
dotnet restore
```

#### Python:

```bash
cd py_scripts
pip install -r requirements.txt
```

---

### 2️⃣ Подготовка базы данных

1. Создай базу данных в PostgreSQL:

   ```sql
   CREATE DATABASE smartstorage_db;
   ```

2. Настрой строку подключения в `appsettings.json`:

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Database=smartstorage_db;Username=postgres;Password=yourpassword"
   }
   ```

3. Применяй миграции:

   ```bash
   dotnet ef database update
   ```

После этого создаются все таблицы:
`Users`, `Robots`, `Products`, `InventoryHistory`.

---

### 3️⃣ Запуск сервисов

#### 🖥 Backend:

```bash
cd SmartStorageBackend
dotnet run
```

→ по умолчанию запускается на `http://localhost:5171`

Swagger-документация доступна по адресу:
👉 **[http://localhost:5171/swagger](http://localhost:5171/swagger)**

---

#### 🤖 Robot Emulator:

```bash
cd py_scripts
python robot.py
```

> Эмулятор запускает несколько роботов (по умолчанию 5),
> каждый из которых отправляет данные каждые 10 секунд.

Настройки через переменные окружения:

```
API_URL=http://localhost:5171
ROBOTS_COUNT=5
UPDATE_INTERVAL=10
```

---

#### 🧩 PyRobot Wrapper (если используется):

```bash
cd py_bot_mod
dotnet run
```

---

## 🧭 Проверка работы

1. Перейди в Swagger → `/api/auth/login`
2. Авторизуйся как робот:

   ```json
   {
     "email": "robot1@local",
     "password": "robotpassword123"
   }
   ```
3. Нажми Authorize → вставь токен → протестируй эндпоинты.
4. Запусти `robot.py` → убедись, что данные появляются в таблице `InventoryHistory`.

---

## 🧩 API — краткая документация

### 🔹 `POST /api/auth/login`

**Авторизация пользователя или робота.**

**Тело запроса:**

```json
{
  "email": "robot1@local",
  "password": "robotpassword123"
}
```

**Ответ:**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs..."
}
```

---

### 🔹 `POST /api/robots/data`

**Описание:** приём данных от робота.

**Авторизация:** Bearer JWT (`Role: robot`)

**Пример запроса:**

```json
{
  "robotId": "RB-001",
  "timestamp": "2025-11-01T10:30:00Z",
  "location": { "zone": "A", "row": 1, "shelf": 2 },
  "scanResults": [
    { "productId": "TEL-4567", "productName": "Роутер RT-AC68U", "quantity": 15, "status": "LOW_STOCK" }
  ],
  "batteryLevel": 86.5,
  "nextCheckpoint": "A-2-1"
}
```

**Ответ:**

```json
{ "status": "received", "message_id": "c3e1f0a3-0c91-4bcd-a682-25e92d4b7a22" }
```

---

### 🔹 `GET /api/inventory/history`

**Описание:** получение истории сканирований.

| Параметр   | Тип      | Пример                 | Описание          |
| ---------- | -------- | ---------------------- | ----------------- |
| `from`     | datetime | `2025-10-31T00:00:00Z` | Начало периода    |
| `to`       | datetime | `2025-11-01T00:00:00Z` | Конец периода     |
| `zone`     | string   | `A`                    | Фильтр по зоне    |
| `status`   | string   | `LOW_STOCK`            | Фильтр по статусу |
| `page`     | int      | `1`                    | Номер страницы    |
| `pageSize` | int      | `20`                   | Размер страницы   |

**Ответ:**

```json
{
  "total": 10,
  "items": [
    {
      "robotId": "RB-001",
      "productId": "TEL-4567",
      "quantity": 15,
      "zone": "A",
      "status": "LOW_STOCK",
      "scannedAt": "2025-11-01T10:30:00Z"
    }
  ],
  "pagination": { "page": 1, "pageSize": 20, "totalPages": 1 }
}
```

---

### 🔹 `POST /api/inventory/import`

**Описание:** импорт данных из CSV-файла.
Формат CSV:

```
product_id;product_name;quantity;zone;date;row;shelf
TEL-1234;WiFi роутер;12;A;2025-11-01;1;2
TEL-2345;Свитч 8 портов;0;A;2025-11-01;1;3
```

**Ответ:**

```json
{ "success": 12, "failed": 0, "errors": [] }
```

---

### 🔹 `GET /api/robots`

(при необходимости)
**Описание:** получение статусов всех роботов.

**Ответ:**

```json
[
  { "id": "RB-001", "status": "active", "batteryLevel": 97 },
  { "id": "RB-002", "status": "active", "batteryLevel": 83 }
]
```

---

## 📡 WebSocket `/api/ws/dashboard`

Используется для live-обновлений на фронтенде (через **SignalR**).

События:

* `robot_update` — обновление позиции и заряда робота
* `inventory_alert` — уведомление о критическом остатке

---

## 💡 Примечания

* Все роботы авторизуются через своих пользователей (`robot1@local`, `robot2@local`, …)
* Роботы создаются автоматически при первом подключении
* Если нужно добавить новых — просто дополни блок инициализации в `Program.cs`

---

## 🧰 Полезные команды

| Команда                           | Назначение                 |
| --------------------------------- | -------------------------- |
| `dotnet ef migrations add <Name>` | создать миграцию           |
| `dotnet ef database update`       | применить миграции         |
| `python robot.py`                 | запустить эмулятор роботов |
| `dotnet run`                      | запустить backend          |

---

## ✨ Контакты

**Автор:** Иван
**Email:** [ivan20140767@gmail.com](mailto:ivan20140767@gmail.com)
**Версия:** v1.0.0
**Дата обновления:** 1 ноября 2025 г.

---
