# Инструкция по работе с моками API

## Текущее состояние

Все API запросы в данный момент используют **моки (тестовые данные)** вместо реальных запросов к backend.
Это позволяет протестировать интерфейс визуально без запущенного backend сервера.

## Как включить реальные API запросы

Когда backend будет готов, выполните следующие шаги:

### 1. Файл `auth.ts`

Раскомментируйте строки:
```typescript
// import { apiClient } from './client' // TODO: раскомментировать когда будет готов бэкенд

// в методе login:
// const response = await apiClient.post<LoginResponse>('/api/auth/login', credentials)
// return response.data
```

Удалите мок-код:
```typescript
// Мок данных для тестирования
return new Promise((resolve) => {
	// ... весь блок с setTimeout
})
```

### 2. Файл `dashboard.ts`

Раскомментируйте:
```typescript
// import { apiClient } from './client' // TODO: раскомментировать когда будет готов бэкенд

// в методе getCurrentData:
// const response = await apiClient.get<DashboardData>('/api/dashboard/current')
// return response.data

// в методе getAIPredictions:
// const response = await apiClient.post<AIPredictionResponse>('/api/ai/predict', {
// 	period_days: _periodDays,
// 	categories: _categories || [],
// })
// return response.data
```

Удалите мок-блоки с `setTimeout`.

### 3. Файл `inventory.ts`

Раскомментируйте:
```typescript
// import { apiClient } from './client' // TODO: раскомментировать когда будет готов бэкенд

// в методе getHistory:
// const params = new URLSearchParams()
// ... весь блок построения параметров
// const response = await apiClient.get<HistoryResponse>(`/api/inventory/history?${params.toString()}`)
// return response.data

// в методе uploadCSV:
// const formData = new FormData()
// formData.append('file', _file)
// const response = await apiClient.post<CSVUploadResponse>('/api/inventory/import', formData, {
// 	headers: {
// 		'Content-Type': 'multipart/form-data',
// 	},
// })
// return response.data

// в методе exportExcel:
// const response = await apiClient.get(`/api/export/excel?ids=${ids.join(',')}`, {
// 	responseType: 'blob',
// })
// return response.data
```

Удалите мок-блоки с `setTimeout`.

## Настройка URL backend

Убедитесь, что в файле `.env` указан правильный адрес backend:

```env
VITE_API_URL=http://localhost:3000
VITE_WS_URL=ws://localhost:3000
```

Измените при необходимости на адрес вашего backend сервера.

## Текущие мок-данные

### Login
- Принимает любой email и пароль
- Возвращает токен и пользователя с ролью "operator"

### Dashboard
- 5 роботов (3 активных, 1 с низким зарядом, 1 оффлайн)
- 5 последних сканирований
- Статистика по складу
- 5 прогнозов ИИ с уровнем достоверности 87.5%

### History
- Генерирует 100 тестовых записей
- Фильтрация по зонам, статусам и поиску работает корректно
- Пагинация работает

### CSV Upload
- Имитирует успешную загрузку
- Возвращает 156 успешных и 3 неудачных записи

### Excel Export
- Возвращает простой CSV файл в виде blob

## Поиск TODO комментариев

Все места, где нужно заменить моки на реальные запросы, помечены комментарием:
```
// TODO: когда будет готов бэкенд
```

Используйте поиск по проекту для быстрого нахождения всех таких мест.

