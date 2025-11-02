# Простая модель на основе линейной регрессии
from sklearn.linear_model import LinearRegression
import pandas as pd
import numpy as np

def predict_stockout(product_history):

# Подготовка данных

    df = pd.DataFrame(product_history)
    X = df['days_ago'].values.reshape(-1, 1)
    y = df['quantity'].values

    # Обучение модели
    model = LinearRegression()
    model.fit(X, y)

    # Прогноз на 7 дней
    future_days = np.array(range(-7, 0)).reshape(-1, 1)
    predictions = model.predict(future_days)

    # Поиск дня исчерпания запасов
    stockout_day = next((i for i, q in enumerate(predictions) if q <= 0), None)

    return {
    'days_until_stockout': stockout_day,
    'recommended_order': calculate_optimal_order(product_history)
    }

