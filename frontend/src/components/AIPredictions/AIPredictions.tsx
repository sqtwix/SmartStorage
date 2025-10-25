import { useState } from 'react'

import { Button, Loader } from '@/shared'
import { AIPrediction } from '@/types'
import { formatDate } from '@/utils'

import './AIPredictions.css'

interface AIPredictionsProps {
	predictions: AIPrediction[]
	confidence: number
	onRefresh: () => void
	loading?: boolean
}

export const AIPredictions = ({ predictions, confidence, onRefresh, loading = false }: AIPredictionsProps) => {
	return (
		<div className="ai-predictions">
			<div className="ai-predictions-header">
				<div>
					<h3 className="ai-predictions-title">Прогноз ИИ на следующие 7 дней</h3>
					<div className="ai-confidence">Достоверность: {confidence}%</div>
				</div>
				<Button variant="secondary" onClick={onRefresh} disabled={loading}>
					{loading ? 'Обновление...' : 'Обновить'}
				</Button>
			</div>

			<div className="ai-predictions-list">
				{loading ? (
					<div className="ai-loading">
						<Loader size="medium" />
					</div>
				) : predictions.length > 0 ? (
					predictions.slice(0, 5).map((prediction) => (
						<div key={prediction.product_id} className="prediction-item">
							<div className="prediction-product">
								<div className="prediction-product-name">{prediction.product_name}</div>
								<div className="prediction-product-id">{prediction.product_id}</div>
							</div>
							<div className="prediction-details">
								<div className="prediction-detail">
									<span className="detail-label">Текущий остаток:</span>
									<span className="detail-value">{prediction.current_stock} ед.</span>
								</div>
								<div className="prediction-detail">
									<span className="detail-label">Дней до исчерпания:</span>
									<span className="detail-value critical">{prediction.days_until_stockout} дней</span>
								</div>
								<div className="prediction-detail">
									<span className="detail-label">Рекомендуемый заказ:</span>
									<span className="detail-value recommended">{prediction.recommended_order} ед.</span>
								</div>
							</div>
						</div>
					))
				) : (
					<div className="no-predictions">Нет критических прогнозов</div>
				)}
			</div>
		</div>
	)
}

