#!/bin/bash
set -e

echo "Installing Python dependencies..."
pip3 install requests || true

echo "Starting robot.py script..."
cd /app/py-scripts
python3 robot.py &
ROBOT_PID=$!

echo "Robot script started with PID: $ROBOT_PID"

# Бесконечный цикл, чтобы контейнер не завершился
while kill -0 $ROBOT_PID 2>/dev/null; do
    sleep 1
done

echo "Robot script has stopped"
exit 1
