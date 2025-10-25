import { useState } from 'react'

import { Badge } from '@/shared'
import { InventoryScan } from '@/types'
import { formatTime } from '@/utils'

import './RecentScans.css'

interface RecentScansProps {
	scans: InventoryScan[]
}

export const RecentScans = ({ scans }: RecentScansProps) => {
	const [paused, setPaused] = useState(false)

	const displayedScans = paused ? scans : scans.slice(0, 20)

	return (
		<div className="recent-scans">
			<div className="recent-scans-header">
				<h3 className="recent-scans-title">Последние сканирования</h3>
				<button className="pause-btn" onClick={() => setPaused(!paused)}>
					{paused ? '▶' : '⏸'}
				</button>
			</div>

			<div className="recent-scans-table-container">
				<table className="recent-scans-table">
					<thead>
						<tr>
							<th>Время</th>
							<th>ID робота</th>
							<th>Зона</th>
							<th>Товар</th>
							<th>Количество</th>
							<th>Статус</th>
						</tr>
					</thead>
					<tbody>
						{displayedScans.map((scan) => (
							<tr key={scan.id} className="scan-row">
								<td>{formatTime(scan.scanned_at)}</td>
								<td>{scan.robot_id}</td>
								<td>{scan.zone}</td>
								<td>
									<div className="product-info">
										<div className="product-name">{scan.product_name}</div>
										<div className="product-id">{scan.product_id}</div>
									</div>
								</td>
								<td>{scan.quantity}</td>
								<td>
									<Badge status={scan.status}>{scan.status}</Badge>
								</td>
							</tr>
						))}
					</tbody>
				</table>

				{displayedScans.length === 0 && <div className="no-data">Нет данных о сканированиях</div>}
			</div>
		</div>
	)
}

