# robot_emulator/emulator.py

import json
import time
import random
import requests
from datetime import datetime
import os
import threading

class RobotEmulator:
    def __init__(self, robot_id: str, api_url: str, password: str = "robotpassword123"):
        self.robot_id = robot_id
        self.api_url = api_url.rstrip("/")
        self.password = password
        self.battery = 100.0
        self.current_zone = 'A'
        self.current_row = 1
        self.current_shelf = 1
        self.token = None
        self.token_acquired_at = None
        self.token_ttl_seconds = 8 * 3600  # по ТЗ 8 часов токен, можно изменить

        # Пример товаров
        self.products = [
            {"id": "TEL-4567", "name": "Роутер RT-AC68U"},
            {"id": "TEL-8901", "name": "Модем DSL-2640U"},
            {"id": "TEL-2345", "name": "Коммутатор SG-108"},
            {"id": "TEL-6789", "name": "IP-телефон T46S"},
            {"id": "TEL-3456", "name": "Кабель UTP Cat6"}
        ]

    def login(self):
        """Выполняет login к /api/auth/login и сохраняет токен."""
        email = f"{self.robot_id.lower()}@robots.local"  # rb-001@robots.local
        payload = {
            "email": email,
            "password": self.password
        }
        try:
            resp = requests.post(f"{self.api_url}/api/auth/login", json=payload, timeout=10)
            if resp.status_code == 200:
                data = resp.json()
                token = data.get("token") or data.get("Token") or data.get("token".lower())
                if token:
                    self.token = token
                    self.token_acquired_at = datetime.utcnow()
                    print(f"[{self.robot_id}] Logged in successfully.")
                    return True
                else:
                    print(f"[{self.robot_id}] Login response missing token: {data}")
            else:
                print(f"[{self.robot_id}] Login failed: {resp.status_code} {resp.text}")
        except Exception as e:
            print(f"[{self.robot_id}] Login error: {e}")
        return False

    def token_is_valid(self):
        """Простая проверка времени жизни токена (рефреш при необходимости)."""
        if not self.token or not self.token_acquired_at:
            return False
        elapsed = (datetime.utcnow() - self.token_acquired_at).total_seconds()
        return elapsed < (self.token_ttl_seconds - 60)  # рефреш за 60 сек до экспирации

    def generate_scan_data(self):
        """Генерация данных сканирования."""
        scanned_products = random.sample(self.products, k=random.randint(1, 3))
        scan_results = []
        for product in scanned_products:
            quantity = random.randint(0, 100)
            status = "OK" if quantity > 20 else ("LOW_STOCK" if quantity > 10 else "CRITICAL")
            scan_results.append({
                "product_id": product["id"],
                "product_name": product["name"],
                "quantity": quantity,
                "status": status
            })
        return scan_results

    def move_to_next_location(self):
        """Перемещение робота к следующей локации (циклично)."""
        self.current_shelf += 1
        if self.current_shelf > 10:
            self.current_shelf = 1
            self.current_row += 1
            if self.current_row > 20:
                self.current_row = 1
                # переход к следующей зоне
                self.current_zone = chr(ord(self.current_zone) + 1)
                if ord(self.current_zone) > ord('Z'):
                    self.current_zone = 'A'
        # расход батареи
        self.battery -= random.uniform(0.1, 0.5)
        if self.battery < 20:
            # симуляция зарядки/догрузки
            self.battery = 100.0

    def send_data(self):
        """Отправляет POST /api/robots/data с токеном. Если 401 — ре-логинится."""
        if not self.token_is_valid():
            ok = self.login()
            if not ok:
                print(f"[{self.robot_id}] Cannot login, skipping send.")
                return

        data = {
            "RobotId": self.robot_id,
            "timestamp": datetime.utcnow().isoformat() + "Z",
            "location": {
                "zone": self.current_zone,
                "row": self.current_row,
                "shelf": self.current_shelf
            },
            "scan_results": self.generate_scan_data(),
            "battery_level": round(self.battery, 1),
            "next_checkpoint": f"{self.current_zone}-{self.current_row+1}-{self.current_shelf}"
        }

        headers = {
            "Authorization": f"Bearer {self.token}",
            "Content-Type": "application/json"
        }

        try:
            resp = requests.post(f"{self.api_url}/api/robots/data", json=data, headers=headers, timeout=10)
            if resp.status_code == 200:
                print(f"[{self.robot_id}] Data sent successfully.")
            elif resp.status_code == 401:
                print(f"[{self.robot_id}] Unauthorized. Token might be expired. Trying re-login.")
                if self.login():
                    headers["Authorization"] = f"Bearer {self.token}"
                    resp2 = requests.post(f"{self.api_url}/api/robots/data", json=data, headers=headers, timeout=10)
                    print(f"[{self.robot_id}] Retry response: {resp2.status_code} {resp2.text}")
                else:
                    print(f"[{self.robot_id}] Re-login failed.")
            else:
                print(f"[{self.robot_id}] Error: {resp.status_code} {resp.text}")
        except Exception as e:
            print(f"[{self.robot_id}] Connection error: {e}")

    def run(self, interval_seconds=10):
        """Основной цикл работы робота."""
        # Вначале логинимся
        self.login()
        while True:
            try:
                self.send_data()
                self.move_to_next_location()
                time.sleep(interval_seconds)
            except KeyboardInterrupt:
                print(f"[{self.robot_id}] Stopped by user.")
                break
            except Exception as ex:
                print(f"[{self.robot_id}] Run error: {ex}")
                time.sleep(5)

def start_emulators(api_url: str, robots_count: int = 5, interval_seconds: int = 10):
    threads = []
    for i in range(1, robots_count + 1):
        rid = f"RB-{i:03d}"
        r = RobotEmulator(rid, api_url)
        t = threading.Thread(target=r.run, args=(interval_seconds,), daemon=True)
        t.start()
        threads.append(t)
    return threads

if __name__ == "__main__":
    api_url = os.getenv("API_URL", "http://localhost:5171")  # адрес вашего ASP.NET backend
    robots_count = int(os.getenv("ROBOTS_COUNT", "5"))
    interval_seconds = int(os.getenv("UPDATE_INTERVAL", "10"))

    print(f"Starting {robots_count} robot emulators -> {api_url}")
    start_emulators(api_url, robots_count, interval_seconds)

    # блокируем главный поток, чтобы демоны работали
    try:
        while True:
            time.sleep(60)
    except KeyboardInterrupt:
        print("Stopping emulators.")
