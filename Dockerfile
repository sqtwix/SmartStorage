FROM python:3.9-slim

WORKDIR /app

# Установка зависимостей
RUN apt-get update && apt-get install -y \
    gcc \
    && rm -rf /var/lib/apt/lists/*

# Копирование requirements и установка Python пакетов
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Создание директории для скриптов
RUN mkdir -p /app/scripts

# Копирование основного скрипта запуска
COPY robot.py .

# Запуск скрипта
CMD ["python", "robot.py"]