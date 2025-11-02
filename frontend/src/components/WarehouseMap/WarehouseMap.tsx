// ИЗМЕНЕНИЕ 1: Добавлены хуки useEffect и useRef для управления DOM-элементами и обработки событий
// - useRef: нужен для получения прямой ссылки на DOM-элемент контейнера карты
// - useEffect: нужен для подписки на события прокрутки колесиком мыши
import { useEffect, useRef, useState } from 'react'

import { getRobotStatusColor } from '@/data-layer'
import { InventoryScan, Robot } from '@/types'

import './WarehouseMap.css'

interface WarehouseMapProps {
	robots: Robot[]
	recentScans?: InventoryScan[]
}

export const WarehouseMap = ({ robots, recentScans = [] }: WarehouseMapProps) => {
	const [scale, setScale] = useState(0.5)
	const [hoveredRobot, setHoveredRobot] = useState<string | null>(null)
	
	// ИЗМЕНЕНИЕ 2: Создание ref для получения доступа к DOM-элементу контейнера карты
	// Это необходимо для:
	// - Программного управления прокруткой (scrollLeft, scrollTop)
	// - Подписки на события прокрутки колесиком мыши
	// Тип HTMLDivElement указывает, что ref будет хранить ссылку на div-элемент
	const containerRef = useRef<HTMLDivElement>(null)

	// A-Z зоны (26 зон)
	const zones = Array.from({ length: 26 }, (_, i) => String.fromCharCode(65 + i))
	const rows = 50
	const cols = 10
	const cellWidth = 25
	const cellHeight = 18
	const zoneWidth = cols * cellWidth + 20 // ширина зоны с отступами
	const mapWidth = zones.length * zoneWidth
	const mapHeight = rows * cellHeight + 40 // высота карты с заголовками

	const handleZoomIn = () => setScale(Math.min(scale + 0.2, 2))
	const handleZoomOut = () => setScale(Math.max(scale - 0.2, 0.3))
	
	// ИЗМЕНЕНИЕ 3: Расширена функциональность кнопки центрирования карты
	// Теперь функция не только сбрасывает масштаб, но и:
	// 1. Устанавливает масштаб обратно к 0.5 (начальное значение)
	// 2. Сбрасывает позицию прокрутки контейнера к началу (0, 0)
	// Это необходимо, потому что при больших картах пользователь может прокрутить карту,
	// и после центрирования нужно вернуть его к начальной позиции (левый верхний угол)
	const handleCenter = () => {
		setScale(0.5)
		// Проверяем, что ref содержит ссылку на DOM-элемент (не null)
		// Это безопасная проверка, так как при первом рендере элемент может быть еще не создан
		if (containerRef.current) {
			// scrollLeft = 0 - прокрутка по горизонтали к самому левому краю
			containerRef.current.scrollLeft = 0
			// scrollTop = 0 - прокрутка по вертикали к самому верхнему краю
			containerRef.current.scrollTop = 0
		}
	}

	// ИЗМЕНЕНИЕ 4: Добавлен обработчик для горизонтальной прокрутки колесиком мыши
	// Это улучшение UX - позволяет прокручивать широкую карту горизонтально
	useEffect(() => {
		// Получаем ссылку на контейнер из ref
		const container = containerRef.current
		// Если контейнер еще не создан (null), выходим из эффекта
		// Это может произойти при первом рендере, когда DOM еще не готов
		if (!container) return

		// Обработчик события прокрутки колесиком мыши
		const handleWheel = (e: WheelEvent) => {
			// Условие для горизонтальной прокрутки:
			// 1. e.shiftKey - если зажат Shift + колесико мыши, это обычно означает горизонтальную прокрутку
			// 2. Math.abs(e.deltaX) > Math.abs(e.deltaY) - если горизонтальное движение больше вертикального
			//    (например, при использовании трекпада с жестами)
			if (e.shiftKey || Math.abs(e.deltaX) > Math.abs(e.deltaY)) {
				// Предотвращаем стандартное поведение прокрутки браузера (вертикальную прокрутку страницы)
				e.preventDefault()
				// Прокручиваем контейнер горизонтально:
				// e.deltaY - вертикальное движение колесика (преобразуем в горизонтальное)
				// e.deltaX - горизонтальное движение (например, от трекпада)
				// Суммируем оба значения для плавной прокрутки в любом случае
				container.scrollLeft += e.deltaY + e.deltaX
			}
			// Если условие не выполнено, стандартная вертикальная прокрутка остается активной
		}

		// Подписываемся на событие 'wheel' (прокрутка колесиком мыши)
		// { passive: false } - важно! Позволяет вызывать preventDefault() для предотвращения стандартного поведения
		// Если бы было passive: true, preventDefault() не работал бы
		container.addEventListener('wheel', handleWheel, { passive: false })
		
		// Cleanup функция - вызывается при размонтировании компонента или изменении зависимостей
		// Важно удалить обработчик, чтобы избежать утечек памяти
		// Пустой массив зависимостей [] означает, что эффект выполнится только один раз при монтировании
		return () => {
			container.removeEventListener('wheel', handleWheel)
		}
	}, [])

	// Определение статуса зоны на основе сканирований
	const getZoneStatus = (zone: string): 'checked' | 'needs_check' | 'critical' => {
		const zoneScans = recentScans.filter(scan => scan.zone === zone)
		
		if (zoneScans.length === 0) {
			return 'needs_check' // Зона не проверялась - требует проверки
		}

		// Проверяем наличие критических остатков
		const hasCritical = zoneScans.some(scan => scan.status === 'CRITICAL')
		if (hasCritical) {
			return 'critical'
		}

		// Проверяем, когда последний раз проверялась зона
		const lastScan = zoneScans.reduce((latest, scan) => {
			const scanTime = new Date(scan.scanned_at).getTime()
			const latestTime = new Date(latest.scanned_at).getTime()
			return scanTime > latestTime ? scan : latest
		}, zoneScans[0])

		const hoursSinceLastScan = (Date.now() - new Date(lastScan.scanned_at).getTime()) / (1000 * 60 * 60)
		
		if (hoursSinceLastScan > 4) {
			return 'needs_check' // Более 4 часов - требует проверки
		}

		return 'checked' // Проверена недавно
	}

	const getZoneColor = (status: 'checked' | 'needs_check' | 'critical'): string => {
		switch (status) {
		case 'checked':
			return '#c8e6c9' // Зеленая - проверена недавно
		case 'needs_check':
			return '#fff9c4' // Желтая - требует проверки
		case 'critical':
			return '#ffcdd2' // Красная - критические остатки
		default:
			return '#e8f5e9'
		}
	}

	const getRobotPosition = (robot: Robot) => {
		const zoneIndex = zones.indexOf(robot.currentZone)
		if (zoneIndex === -1) {
			// Если зона не найдена, размещаем в зоне A
			return { x: (robot.currentShelf - 1) * cellWidth + 10, y: (robot.currentRow - 1) * cellHeight + 30 }
		}
		const x = zoneIndex * zoneWidth + (robot.currentShelf - 1) * cellWidth + 10
		const y = (robot.currentRow - 1) * cellHeight + 30
		return { x, y }
	}

	const getTooltipPlacement = (robot: Robot) => {
		const pos = getRobotPosition(robot)
		// Определяем, с какой стороны показывать тултип
		// чтобы он не выходил за границы карты
		
		let x = 0
		let y = 0
		let placement = 'top' // top, bottom, left, right
		
		const mapWidthPx = mapWidth * scale
		const mapHeightPx = mapHeight * scale
		
		// Проверяем границы по X
		if (pos.x * scale < 300) {
			// Робот слева - показываем справа
			placement = 'right'
			x = 35
			y = -45
		} else if (pos.x * scale > mapWidthPx - 300) {
			// Робот справа - показываем слева
			placement = 'left'
			x = -315
			y = -45
		} else if (pos.y * scale < 150) {
			// Робот сверху - показываем снизу
			placement = 'bottom'
			x = -120
			y = 40
		} else if (pos.y * scale > mapHeightPx - 200) {
			// Робот снизу - показываем сверху
			placement = 'top'
			x = -120
			y = -165
		} else {
			// По умолчанию сверху
			placement = 'top'
			x = -120
			y = -165
		}
		
		return { x, y, placement }
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

			{/* ИЗМЕНЕНИЕ 5: Добавлен ref к контейнеру карты и изменена структура вложенности */}
			{/* 
				ref={containerRef} - связываем ref с DOM-элементом контейнера
				Это позволяет нам программно управлять прокруткой через JavaScript
			*/}
			<div className="warehouse-map-container" ref={containerRef}>
				{/* 
					Внутренний div-обертка для SVG:
					- Определяет фактический размер области содержимого с учетом масштаба
					- mapWidth * scale - ширина карты, умноженная на текущий масштаб
					- mapHeight * scale - высота карты, умноженная на текущий масштаб
					- Этот div создает область, которая может быть больше видимой области контейнера
					- Контейнер с overflow: hidden/auto будет показывать скроллбары при необходимости
				*/}
				<div 
					className="warehouse-map-inner"
					style={{ 
						width: `${mapWidth * scale}px`,   // Фактическая ширина карты с учетом масштаба
						height: `${mapHeight * scale}px`, // Фактическая высота карты с учетом масштаба
					}}
				>
					{/* 
						SVG элемент с явными размерами вместо CSS transform: scale()
						Преимущества этого подхода:
						1. Размеры SVG реально изменяются, а не визуально масштабируются через CSS
						2. Это позволяет контейнеру правильно рассчитывать размеры для прокрутки
						3. viewBox остается в логических единицах (mapWidth x mapHeight)
						4. width и height устанавливаются в пикселях с учетом масштаба
					*/}
					<svg 
						className="warehouse-map-svg" 
						viewBox={`0 0 ${mapWidth} ${mapHeight}`}  // viewBox в логических единицах (без учета масштаба)
						style={{ 
							width: `${mapWidth * scale}px`,   // Фактическая ширина SVG в пикселях
							height: `${mapHeight * scale}px`, // Фактическая высота SVG в пикселях
						}}
					>
						{/* Grid */}
						{zones.map((zone, zoneIdx) => {
							const zoneStatus = getZoneStatus(zone)
							const zoneColor = getZoneColor(zoneStatus)
							
							return (
								<g key={zone}>
									<text 
										x={zoneIdx * zoneWidth + zoneWidth / 2} 
										y={20} 
										textAnchor="middle" 
										className="zone-label"
									>
										Зона {zone}
									</text>
									{Array.from({ length: rows }).map((_, rowIdx) => (
										<g key={`${zone}-${rowIdx}`}>
											{Array.from({ length: cols }).map((_, colIdx) => (
												<rect
													key={`${zone}-${rowIdx}-${colIdx}`}
													x={zoneIdx * zoneWidth + colIdx * cellWidth + 10}
													y={30 + rowIdx * cellHeight}
													width={cellWidth - 2}
													height={cellHeight - 2}
													className="warehouse-cell"
													fill={zoneColor}
													stroke={zoneStatus === 'critical' ? '#f44336' : zoneStatus === 'needs_check' ? '#ffc107' : '#a5d6a7'}
													strokeWidth={zoneStatus === 'critical' ? 1.5 : 1}
												/>
											))}
										</g>
									))}
								</g>
							)
						})}

						{/* Robots */}
						{robots.map((robot) => {
							const pos = getRobotPosition(robot)
							const color = getRobotStatusColor(robot.status)
							const isHovered = hoveredRobot === robot.id
							const tooltipPlacement = getTooltipPlacement(robot)

							return (
								<g
									key={robot.id}
									transform={`translate(${pos.x}, ${pos.y})`}
									onMouseEnter={() => setHoveredRobot(robot.id)}
									onMouseLeave={() => setHoveredRobot(null)}
									className="robot-marker"
								>
									<circle cx={cellWidth / 2} cy={cellHeight / 2} r={12} fill={color} />
									<text x={cellWidth / 2} y={cellHeight / 2 + 4} textAnchor="middle" className="robot-label" fontSize="12">
										{robot.id.split('-')[1] || robot.id}
									</text>

									{/* Tooltip using foreignObject */}
									{isHovered && (
										<foreignObject
											x={tooltipPlacement.x}
											y={tooltipPlacement.y}
											width={300}
											height={150}
											style={{ overflow: 'visible' }}
										>
											<div className={`robot-tooltip robot-tooltip-${tooltipPlacement.placement}`}>
												<div className="robot-tooltip-content">
													<div>ID: {robot.id}</div>
													<div>Батарея: {robot.batteryLevel}%</div>
													<div>Обновлено: {new Date(robot.lastUpdate).toLocaleTimeString('ru')}</div>
												</div>
											</div>
										</foreignObject>
									)}
								</g>
							)
						})}
					</svg>
				</div>
			</div>

			<div className="warehouse-map-legend">
				<div className="legend-section">
					<div className="legend-title">Роботы:</div>
					<div className="legend-items">
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
				<div className="legend-section">
					<div className="legend-title">Зоны:</div>
					<div className="legend-items">
						<div className="legend-item">
							<span className="legend-dot" style={{ background: '#c8e6c9' }}></span>
							<span>Проверена недавно</span>
						</div>
						<div className="legend-item">
							<span className="legend-dot" style={{ background: '#fff9c4' }}></span>
							<span>Требует проверки</span>
						</div>
						<div className="legend-item">
							<span className="legend-dot" style={{ background: '#ffcdd2' }}></span>
							<span>Критические остатки</span>
						</div>
					</div>
				</div>
			</div>
		</div>
	)
}

