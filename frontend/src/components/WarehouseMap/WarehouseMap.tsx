import { useState } from 'react'

import { Robot } from '@/types'
import { getRobotStatusColor } from '@/data-layer'

import './WarehouseMap.css'

interface WarehouseMapProps {
	robots: Robot[]
}

export const WarehouseMap = ({ robots }: WarehouseMapProps) => {
	const [scale, setScale] = useState(1)
	const [hoveredRobot, setHoveredRobot] = useState<string | null>(null)

	const zones = ['A', 'B', 'C', 'D', 'E']
	const rows = 20
	const cols = 10

	const handleZoomIn = () => setScale(Math.min(scale + 0.2, 2))
	const handleZoomOut = () => setScale(Math.max(scale - 0.2, 0.6))
	const handleCenter = () => setScale(1)

	const getRobotPosition = (robot: Robot) => {
		const zoneIndex = zones.indexOf(robot.current_zone)
		const x = (zoneIndex * cols + robot.current_shelf) * 40
		const y = robot.current_row * 30
		return { x, y }
	}

	return (
		<div className="warehouse-map">
			<div className="warehouse-map-header">
				<h3 className="warehouse-map-title">Карта склада</h3>
				<div className="warehouse-map-controls">
					<button className="map-control-btn" onClick={handleZoomOut}>
						−
					</button>
					<button className="map-control-btn" onClick={handleCenter}>
						◎
					</button>
					<button className="map-control-btn" onClick={handleZoomIn}>
						+
					</button>
				</div>
			</div>

			<div className="warehouse-map-container">
				<svg className="warehouse-map-svg" viewBox="0 0 2000 600" style={{ transform: `scale(${scale})` }}>
					{/* Grid */}
					{zones.map((zone, zoneIdx) => (
						<g key={zone}>
							<text x={zoneIdx * 400 + 200} y={20} textAnchor="middle" className="zone-label">
								Зона {zone}
							</text>
							{Array.from({ length: rows }).map((_, rowIdx) => (
								<g key={`${zone}-${rowIdx}`}>
									{Array.from({ length: cols }).map((_, colIdx) => (
										<rect
											key={`${zone}-${rowIdx}-${colIdx}`}
											x={zoneIdx * 400 + colIdx * 40}
											y={30 + rowIdx * 30}
											width={38}
											height={28}
											className="warehouse-cell"
										/>
									))}
								</g>
							))}
						</g>
					))}

					{/* Robots */}
					{robots.map((robot) => {
						const pos = getRobotPosition(robot)
						const color = getRobotStatusColor(robot.status)

						return (
							<g
								key={robot.id}
								transform={`translate(${pos.x}, ${pos.y})`}
								onMouseEnter={() => setHoveredRobot(robot.id)}
								onMouseLeave={() => setHoveredRobot(null)}
								className="robot-marker"
							>
								<circle cx={19} cy={14} r={12} fill={color} />
								<text x={19} y={18} textAnchor="middle" className="robot-label">
									{robot.id.split('-')[1]}
								</text>

								{hoveredRobot === robot.id && (
									<g>
										<rect x={-60} y={-50} width={160} height={80} fill="white" stroke="#ccc" rx={4} />
										<text x={20} y={-30} className="robot-tooltip-text">
											ID: {robot.id}
										</text>
										<text x={20} y={-10} className="robot-tooltip-text">
											Батарея: {robot.battery_level}%
										</text>
										<text x={20} y={10} className="robot-tooltip-text">
											Обновлено: {new Date(robot.last_update).toLocaleTimeString('ru')}
										</text>
									</g>
								)}
							</g>
						)
					})}
				</svg>
			</div>

			<div className="warehouse-map-legend">
				<div className="legend-item">
					<span className="legend-dot" style={{ background: '#4caf50' }}></span>
					<span>Активен</span>
				</div>
				<div className="legend-item">
					<span className="legend-dot" style={{ background: '#ff9800' }}></span>
					<span>Низкий заряд</span>
				</div>
				<div className="legend-item">
					<span className="legend-dot" style={{ background: '#f44336' }}></span>
					<span>Оффлайн</span>
				</div>
			</div>
		</div>
	)
}

