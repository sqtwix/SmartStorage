import { Link, useLocation } from 'react-router-dom'

import { Button } from '@/shared'

import './Navigation.css'

interface NavigationProps {
	onUploadCSV?: () => void
}

export const Navigation = ({ onUploadCSV }: NavigationProps) => {
	const location = useLocation()

	return (
		<nav className="navigation">
			<div className="navigation-tabs">
				<Link to="/dashboard" className={`navigation-tab ${location.pathname === '/dashboard' ? 'active' : ''}`}>
					Текущий мониторинг
				</Link>
				<Link to="/history" className={`navigation-tab ${location.pathname === '/history' ? 'active' : ''}`}>
					Исторические данные
				</Link>
			</div>

			{onUploadCSV && (
				<Button variant="secondary" onClick={onUploadCSV}>
					Загрузить CSV
				</Button>
			)}
		</nav>
	)
}

