import './Badge.css'

interface BadgeProps {
	status: 'OK' | 'LOW_STOCK' | 'CRITICAL' | 'active' | 'low_battery' | 'offline'
	children: React.ReactNode
}

export const Badge = ({ status, children }: BadgeProps) => {
	return <span className={`badge badge-${status.toLowerCase()}`}>{children}</span>
}

