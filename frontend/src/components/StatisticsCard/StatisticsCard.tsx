import './StatisticsCard.css'

interface StatisticsCardProps {
	title: string
	value: string | number
	icon?: string
	color?: string
}

export const StatisticsCard = ({ title, value, icon, color = '#0066cc' }: StatisticsCardProps) => {
	return (
		<div className="statistics-card">
			<div className="statistics-card-icon" style={{ background: color }}>
				{icon || '📊'}
			</div>
			<div className="statistics-card-content">
				<div className="statistics-card-title">{title}</div>
				<div className="statistics-card-value">{value}</div>
			</div>
		</div>
	)
}

