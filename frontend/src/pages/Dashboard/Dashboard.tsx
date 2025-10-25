import { useEffect, useState } from 'react'

import { dashboardApi } from '@/api'
import { AIPredictions } from '@/components/AIPredictions'
import { Header } from '@/components/Header'
import { Navigation } from '@/components/Navigation'
import { RecentScans } from '@/components/RecentScans'
import { StatisticsCard } from '@/components/StatisticsCard'
import { WarehouseMap } from '@/components/WarehouseMap'
import { WebSocketIndicator } from '@/components/WebSocketIndicator'
import { processDashboardData, wsService } from '@/data-layer'
import { Loader } from '@/shared'
import { AIPrediction, DashboardData, InventoryScan, Robot, WebSocketMessage } from '@/types'
import { formatNumber, formatPercent } from '@/utils'

import './Dashboard.css'

export const Dashboard = () => {
	const [data, setData] = useState<DashboardData | null>(null)
	const [predictions, setPredictions] = useState<AIPrediction[]>([])
	const [confidence, setConfidence] = useState(0)
	const [loading, setLoading] = useState(true)
	const [aiLoading, setAiLoading] = useState(false)

	useEffect(() => {
		loadDashboardData()
		loadAIPredictions()

		// WebSocket подключение
		wsService.connect()

		// Подписка на обновления роботов
		wsService.subscribe('robot_update', (message: WebSocketMessage) => {
			if (data && 'id' in message.data) {
				const updatedRobot = message.data as Robot
				setData((prev) =>
					prev
						? {
								...prev,
								robots: prev.robots.map((robot) => (robot.id === updatedRobot.id ? updatedRobot : robot)),
							}
						: null
				)
			}
		})

		// Подписка на новые сканирования
		wsService.subscribe('scan_update', (message: WebSocketMessage) => {
			if (data && 'product_id' in message.data) {
				const newScan = message.data as InventoryScan
				setData((prev) =>
					prev
						? {
								...prev,
								recent_scans: [newScan, ...prev.recent_scans].slice(0, 20),
							}
						: null
				)
			}
		})

		// Периодическое обновление
		const interval = setInterval(() => {
			loadDashboardData()
		}, 5000)

		return () => {
			clearInterval(interval)
			wsService.disconnect()
		}
	}, [])

	const loadDashboardData = async () => {
		try {
			const response = await dashboardApi.getCurrentData()
			const processed = processDashboardData(response)
			setData(processed)
		} catch (error) {
			console.error('Failed to load dashboard data:', error)
		} finally {
			setLoading(false)
		}
	}

	const loadAIPredictions = async () => {
		setAiLoading(true)
		try {
			const response = await dashboardApi.getAIPredictions(7)
			setPredictions(response.predictions)
			setConfidence(response.confidence)
		} catch (error) {
			console.error('Failed to load AI predictions:', error)
		} finally {
			setAiLoading(false)
		}
	}

	if (loading || !data) {
		return <Loader fullScreen size="large" />
	}

	return (
		<div className="dashboard-page">
			<Header />
			<Navigation />

			<div className="dashboard-content">
				<div className="dashboard-left">
					<WarehouseMap robots={data.robots} />
				</div>

				<div className="dashboard-right">
					<div className="dashboard-statistics">
						<StatisticsCard
							title="Активных роботов"
							value={`${data.statistics.active_robots}/${data.statistics.total_robots}`}
							icon="🤖"
							color="#4caf50"
						/>
						<StatisticsCard
							title="Проверено сегодня"
							value={formatNumber(data.statistics.checked_today)}
							icon="✓"
							color="#2196f3"
						/>
						<StatisticsCard
							title="Критических остатков"
							value={data.statistics.critical_items}
							icon="⚠"
							color="#f44336"
						/>
						<StatisticsCard
							title="Средний заряд батарей"
							value={formatPercent(data.statistics.average_battery)}
							icon="🔋"
							color="#ff9800"
						/>
					</div>

					<div className="dashboard-scans">
						<RecentScans scans={data.recent_scans} />
					</div>

					<div className="dashboard-predictions">
						<AIPredictions
							predictions={predictions}
							confidence={confidence}
							onRefresh={loadAIPredictions}
							loading={aiLoading}
						/>
					</div>
				</div>
			</div>

			<WebSocketIndicator />
		</div>
	)
}

