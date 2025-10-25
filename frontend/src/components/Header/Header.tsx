import { authApi } from '@/api'
import { Button } from '@/shared'

import './Header.css'

export const Header = () => {
	const user = authApi.getStoredUser()

	const handleLogout = () => {
		authApi.logout()
		window.location.href = '/login'
	}

	return (
		<header className="header">
			<div className="header-left">
				<img src="/rostelekom-logo.svg" alt="Ростелеком" className="header-logo" />
				<h1 className="header-title">Умный склад</h1>
			</div>

			<div className="header-right">
				<div className="header-user">
					<span className="header-user-name">{user?.name || 'Пользователь'}</span>
					<span className="header-user-role">{user?.role || 'operator'}</span>
				</div>
				<Button variant="secondary" onClick={handleLogout}>
					Выход
				</Button>
			</div>
		</header>
	)
}

