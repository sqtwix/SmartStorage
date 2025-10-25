import { useEffect, useState } from 'react'

import { inventoryApi } from '@/api'
import { CSVUploadModal } from '@/components/CSVUploadModal'
import { Header } from '@/components/Header'
import { HistoryFilters } from '@/components/HistoryFilters'
import { Navigation } from '@/components/Navigation'
import { downloadBlob, processHistoryRecords } from '@/data-layer'
import { Badge, Button, Loader } from '@/shared'
import { HistoryFilters as HistoryFiltersType, HistoryRecord } from '@/types'
import { formatDateTime, formatNumber } from '@/utils'

import './History.css'

export const History = () => {
	const [records, setRecords] = useState<HistoryRecord[]>([])
	const [total, setTotal] = useState(0)
	const [loading, setLoading] = useState(true)
	const [page, setPage] = useState(1)
	const [limit, setLimit] = useState(20)
	const [filters, setFilters] = useState<HistoryFiltersType>({})
	const [selectedIds, setSelectedIds] = useState<number[]>([])
	const [uploadModalOpen, setUploadModalOpen] = useState(false)
	const [sortField, setSortField] = useState<string | null>(null)
	const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('desc')

	useEffect(() => {
		loadHistory()
	}, [page, limit, filters])

	const loadHistory = async () => {
		setLoading(true)
		try {
			const response = await inventoryApi.getHistory({
				...filters,
				page,
				limit,
			})
			const processed = processHistoryRecords(response.items)
			setRecords(processed)
			setTotal(response.total)
		} catch (error) {
			console.error('Failed to load history:', error)
		} finally {
			setLoading(false)
		}
	}

	const handleApplyFilters = (newFilters: HistoryFiltersType) => {
		setFilters(newFilters)
		setPage(1)
	}

	const handleResetFilters = () => {
		setFilters({})
		setPage(1)
	}

	const handleExportExcel = async () => {
		if (selectedIds.length === 0) return

		try {
			const blob = await inventoryApi.exportExcel(selectedIds)
			downloadBlob(blob, `history_export_${new Date().toISOString().split('T')[0]}.xlsx`)
		} catch (error) {
			console.error('Failed to export:', error)
		}
	}

	const handleSelectAll = (checked: boolean) => {
		if (checked) {
			setSelectedIds(records.map((r) => r.id))
		} else {
			setSelectedIds([])
		}
	}

	const handleSelectOne = (id: number, checked: boolean) => {
		if (checked) {
			setSelectedIds([...selectedIds, id])
		} else {
			setSelectedIds(selectedIds.filter((i) => i !== id))
		}
	}

	const handleSort = (field: string) => {
		if (sortField === field) {
			setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc')
		} else {
			setSortField(field)
			setSortOrder('desc')
		}
	}

	const sortedRecords = [...records].sort((a, b) => {
		if (!sortField) return 0

		let aVal = a[sortField as keyof HistoryRecord]
		let bVal = b[sortField as keyof HistoryRecord]

		if (typeof aVal === 'string') aVal = aVal.toLowerCase()
		if (typeof bVal === 'string') bVal = bVal.toLowerCase()

		if (aVal < bVal) return sortOrder === 'asc' ? -1 : 1
		if (aVal > bVal) return sortOrder === 'asc' ? 1 : -1
		return 0
	})

	const totalPages = Math.ceil(total / limit)

	return (
		<div className="history-page">
			<Header />
			<Navigation onUploadCSV={() => setUploadModalOpen(true)} />

			<div className="history-content">
				<HistoryFilters onApply={handleApplyFilters} onReset={handleResetFilters} />

				<div className="history-summary">
					<div className="summary-item">
						<span className="summary-label">Всего проверок за период:</span>
						<span className="summary-value">{formatNumber(total)}</span>
					</div>
				</div>

				<div className="history-table-card">
					<div className="history-table-header">
						<h3 className="table-title">Данные инвентаризации</h3>
						<div className="table-actions">
							<Button variant="secondary" onClick={handleExportExcel} disabled={selectedIds.length === 0}>
								Экспорт в Excel ({selectedIds.length})
							</Button>
						</div>
					</div>

					{loading ? (
						<div className="history-loading">
							<Loader size="large" />
						</div>
					) : (
						<>
							<div className="history-table-container">
								<table className="history-table">
									<thead>
										<tr>
											<th>
												<input
													type="checkbox"
													checked={selectedIds.length === records.length && records.length > 0}
													onChange={(e) => handleSelectAll(e.target.checked)}
												/>
											</th>
											<th onClick={() => handleSort('date')} className="sortable">
												Дата и время {sortField === 'date' && (sortOrder === 'asc' ? '↑' : '↓')}
											</th>
											<th onClick={() => handleSort('robot_id')} className="sortable">
												ID робота {sortField === 'robot_id' && (sortOrder === 'asc' ? '↑' : '↓')}
											</th>
											<th onClick={() => handleSort('zone')} className="sortable">
												Зона {sortField === 'zone' && (sortOrder === 'asc' ? '↑' : '↓')}
											</th>
											<th>Артикул</th>
											<th>Название товара</th>
											<th onClick={() => handleSort('expected_quantity')} className="sortable">
												Ожидаемое {sortField === 'expected_quantity' && (sortOrder === 'asc' ? '↑' : '↓')}
											</th>
											<th onClick={() => handleSort('actual_quantity')} className="sortable">
												Фактическое {sortField === 'actual_quantity' && (sortOrder === 'asc' ? '↑' : '↓')}
											</th>
											<th onClick={() => handleSort('difference')} className="sortable">
												Расхождение {sortField === 'difference' && (sortOrder === 'asc' ? '↑' : '↓')}
											</th>
											<th>Статус</th>
										</tr>
									</thead>
									<tbody>
										{sortedRecords.map((record) => (
											<tr key={record.id}>
												<td>
													<input
														type="checkbox"
														checked={selectedIds.includes(record.id)}
														onChange={(e) => handleSelectOne(record.id, e.target.checked)}
													/>
												</td>
												<td>{formatDateTime(record.date)}</td>
												<td>{record.robot_id}</td>
												<td>{record.zone}</td>
												<td>{record.product_id}</td>
												<td>{record.product_name}</td>
												<td>{record.expected_quantity}</td>
												<td>{record.actual_quantity}</td>
												<td className={record.difference !== 0 ? 'difference-highlight' : ''}>{record.difference > 0 ? '+' : ''}{record.difference}</td>
												<td>
													<Badge status={record.status}>{record.status}</Badge>
												</td>
											</tr>
										))}
									</tbody>
								</table>
							</div>

							<div className="history-pagination">
								<div className="pagination-info">
									Показано {(page - 1) * limit + 1}-{Math.min(page * limit, total)} из {total}
								</div>
								<div className="pagination-controls">
									<select value={limit} onChange={(e) => setLimit(Number(e.target.value))} className="limit-select">
										<option value={20}>20 на странице</option>
										<option value={50}>50 на странице</option>
										<option value={100}>100 на странице</option>
									</select>
									<Button variant="secondary" onClick={() => setPage(Math.max(1, page - 1))} disabled={page === 1}>
										← Назад
									</Button>
									<span className="page-indicator">
										{page} / {totalPages}
									</span>
									<Button variant="secondary" onClick={() => setPage(Math.min(totalPages, page + 1))} disabled={page === totalPages}>
										Вперед →
									</Button>
								</div>
							</div>
						</>
					)}
				</div>
			</div>

			<CSVUploadModal isOpen={uploadModalOpen} onClose={() => setUploadModalOpen(false)} onSuccess={loadHistory} />
		</div>
	)
}

