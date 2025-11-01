import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr'

import { WebSocketMessage } from '@/types'

export type ConnectionStatus = 'connected' | 'disconnected' | 'reconnecting'

export class WebSocketService {
	private connection: HubConnection | null = null
	private listeners: Map<string, Set<(data: WebSocketMessage) => void>> = new Map()
	private statusListeners: Set<(status: ConnectionStatus) => void> = new Set()

	async connect() {
		if (this.connection?.state === HubConnectionState.Connected) {
			console.log('WebSocket already connected')
			return
		}

		if (this.connection) {
			await this.connection.stop()
			this.connection = null
		}

		const token = localStorage.getItem('auth_token')
		
		// Используем относительный путь - Vite proxy перенаправит на бэкенд
		this.connection = new HubConnectionBuilder()
			.withUrl('/api/ws/dashboard', {
				accessTokenFactory: () => token || '',
			})
			.withAutomaticReconnect()
			.build()

		console.log('socket', this.connection)

		// Обработчики событий SignalR
		this.connection.onreconnecting(() => {
			console.log('reconnecting')
			this.notifyStatusChange('reconnecting')
		})

		this.connection.onreconnected(() => {
			console.log('reconnected')
			this.notifyStatusChange('connected')
		})

		this.connection.onclose(() => {
			console.log('disconnected')
			this.notifyStatusChange('disconnected')
		})

		// Подписка на события от сервера
		this.connection.on('robot_update', (data: WebSocketMessage['data']) => {
			this.notifyListeners('robot_update', { type: 'robot_update', data })
		})

		this.connection.on('inventory_alert', (data: WebSocketMessage['data']) => {
			this.notifyListeners('inventory_alert', { type: 'inventory_alert', data })
		})

		this.connection.on('scan_update', (data: WebSocketMessage['data']) => {
			this.notifyListeners('scan_update', { type: 'scan_update', data })
		})

		// Запуск подключения
		try {
			await this.connection.start()
			console.log('WebSocket connected')
			this.notifyStatusChange('connected')
		} catch (error) {
			console.error('WebSocket connection failed:', error)
			this.notifyStatusChange('disconnected')
		}
	}

	async disconnect() {
		if (this.connection) {
			await this.connection.stop()
			this.connection = null
		}
	}

	subscribe(eventType: string, callback: (data: WebSocketMessage) => void) {
		if (!this.listeners.has(eventType)) {
			this.listeners.set(eventType, new Set())
		}
		this.listeners.get(eventType)?.add(callback)
	}

	unsubscribe(eventType: string, callback: (data: WebSocketMessage) => void) {
		this.listeners.get(eventType)?.delete(callback)
	}

	subscribeToStatus(callback: (status: ConnectionStatus) => void) {
		this.statusListeners.add(callback)
	}

	unsubscribeFromStatus(callback: (status: ConnectionStatus) => void) {
		this.statusListeners.delete(callback)
	}

	private notifyListeners(eventType: string, data: WebSocketMessage) {
		const listeners = this.listeners.get(eventType)
		if (listeners) {
			listeners.forEach((callback) => callback(data))
		}
	}

	private notifyStatusChange(status: ConnectionStatus) {
		this.statusListeners.forEach((callback) => callback(status))
	}

	isConnected(): boolean {
		return this.connection?.state === HubConnectionState.Connected || false
	}
}

export const wsService = new WebSocketService()

