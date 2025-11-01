import { useEffect, useState } from 'react'

import { ConnectionStatus, wsService } from '@/data-layer'

import './WebSocketIndicator.css'

export const WebSocketIndicator = () => {
	const [status, setStatus] = useState<ConnectionStatus>('disconnected')

	useEffect(() => {
		const handleStatusChange = (newStatus: ConnectionStatus) => {
			setStatus(newStatus)
		}

		wsService.subscribeToStatus(handleStatusChange)

		return () => {
			wsService.unsubscribeFromStatus(handleStatusChange)
		}
	}, [])

	const getStatusColor = () => {
		switch (status) {
		case 'connected':
			return '#4caf50'
		case 'disconnected':
			return '#f44336'
		case 'reconnecting':
			return '#9e9e9e'
		}
	}

	const getStatusText = () => {
		switch (status) {
		case 'connected':
			return 'Подключено'
		case 'disconnected':
			return 'Отключено'
		case 'reconnecting':
			return 'Переподключение...'
		}
	}

	return (
		<div className="ws-indicator">
			<span className="ws-indicator-dot" style={{ background: getStatusColor() }}></span>
			<span className="ws-indicator-text">{getStatusText()}</span>
		</div>
	)
}

