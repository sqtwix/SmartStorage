import { useState } from 'react'

import { Button, Input } from '@/shared'
import { HistoryFilters as HistoryFiltersType } from '@/types'

import './HistoryFilters.css'

interface HistoryFiltersProps {
	onApply: (filters: HistoryFiltersType) => void
	onReset: () => void
}

export const HistoryFilters = ({ onApply, onReset }: HistoryFiltersProps) => {
	const [fromDate, setFromDate] = useState('')
	const [toDate, setToDate] = useState('')
	const [search, setSearch] = useState('')
	const [selectedZones, setSelectedZones] = useState<string[]>([])
	const [selectedStatuses, setSelectedStatuses] = useState<string[]>(['OK', 'LOW_STOCK', 'CRITICAL'])

	const zones = ['A', 'B', 'C', 'D', 'E']
	const statuses = [
		{ value: 'OK', label: 'ОК' },
		{ value: 'LOW_STOCK', label: 'Низкий остаток' },
		{ value: 'CRITICAL', label: 'Критично' },
	]

	const handleQuickFilter = (days: number) => {
		const to = new Date()
		const from = new Date()
		from.setDate(from.getDate() - days)

		setFromDate(from.toISOString().split('T')[0])
		setToDate(to.toISOString().split('T')[0])
	}

	const handleApply = () => {
		onApply({
			from: fromDate || undefined,
			to: toDate || undefined,
			zones: selectedZones.length > 0 ? selectedZones : undefined,
			status: selectedStatuses.length > 0 ? (selectedStatuses as ('OK' | 'LOW_STOCK' | 'CRITICAL')[]) : undefined,
			search: search || undefined,
		})
	}

	const handleReset = () => {
		setFromDate('')
		setToDate('')
		setSearch('')
		setSelectedZones([])
		setSelectedStatuses(['OK', 'LOW_STOCK', 'CRITICAL'])
		onReset()
	}

	const toggleZone = (zone: string) => {
		setSelectedZones((prev) => (prev.includes(zone) ? prev.filter((z) => z !== zone) : [...prev, zone]))
	}

	const toggleStatus = (status: string) => {
		setSelectedStatuses((prev) => (prev.includes(status) ? prev.filter((s) => s !== status) : [...prev, status]))
	}

	return (
		<div className="history-filters">
			<div className="filters-row">
				<div className="filter-group">
					<label className="filter-label">Период</label>
					<div className="date-inputs">
						<Input type="date" value={fromDate} onChange={(e) => setFromDate(e.target.value)} placeholder="От" />
						<Input type="date" value={toDate} onChange={(e) => setToDate(e.target.value)} placeholder="До" />
					</div>
				</div>

				<div className="filter-group">
					<label className="filter-label">Быстрые фильтры</label>
					<div className="quick-filters">
						<button className="quick-filter-btn" onClick={() => handleQuickFilter(0)}>
							Сегодня
						</button>
						<button className="quick-filter-btn" onClick={() => handleQuickFilter(1)}>
							Вчера
						</button>
						<button className="quick-filter-btn" onClick={() => handleQuickFilter(7)}>
							Неделя
						</button>
						<button className="quick-filter-btn" onClick={() => handleQuickFilter(30)}>
							Месяц
						</button>
					</div>
				</div>
			</div>

			<div className="filters-row">
				<div className="filter-group">
					<label className="filter-label">Зоны склада</label>
					<div className="zone-checkboxes">
						{zones.map((zone) => (
							<label key={zone} className="checkbox-label">
								<input type="checkbox" checked={selectedZones.includes(zone)} onChange={() => toggleZone(zone)} />
								<span>Зона {zone}</span>
							</label>
						))}
					</div>
				</div>

				<div className="filter-group">
					<label className="filter-label">Статус</label>
					<div className="status-checkboxes">
						{statuses.map((status) => (
							<label key={status.value} className="checkbox-label">
								<input type="checkbox" checked={selectedStatuses.includes(status.value)} onChange={() => toggleStatus(status.value)} />
								<span>{status.label}</span>
							</label>
						))}
					</div>
				</div>
			</div>

			<div className="filters-row">
				<div className="filter-group" style={{ flex: 1 }}>
					<label className="filter-label">Поиск по артикулу или названию</label>
					<Input type="text" value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Введите артикул или название" />
				</div>

				<div className="filter-actions">
					<Button variant="secondary" onClick={handleReset}>
						Сбросить
					</Button>
					<Button variant="primary" onClick={handleApply}>
						Применить
					</Button>
				</div>
			</div>
		</div>
	)
}

