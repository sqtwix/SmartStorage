import { io, Socket } from 'socket.io-client'

import { WebSocketMessage } from '@/types'

const WS_URL = import.meta.env.VITE_WS_URL || 'ws://localhost:3000'

export type ConnectionStatus = 'connected' | 'disconnected' | 'reconnecting'

export class WebSocketService {
	private socket: Socket | null = null
	private listeners: Map<string, Set<(data: WebSocketMessage) => void>> = new Map()
	private statusListeners: Set<(status: ConnectionStatus) => void> = new Set()

	connect() {
		if (this.socket?.connected) {
			return
		}

		const token = localStorage.getItem('auth_token')

		this.socket = io(WS_URL, {
			path: '/api/ws/dashboard',
			auth: {
				token,
			},
			transports: ['websocket'],
		})

		this.socket.on('connect', () => {
			this.notifyStatusChange('connected')
		})

		this.socket.on('disconnect', () => {
			this.notifyStatusChange('disconnected')
		})

		this.socket.on('reconnecting', () => {
			this.notifyStatusChange('reconnecting')
		})

		this.socket.on('message', (message: WebSocketMessage) => {
			this.notifyListeners(message.type, message)
		})

		this.socket.on('robot_update', (data: WebSocketMessage) => {
			this.notifyListeners('robot_update', data)
		})

		this.socket.on('inventory_alert', (data: WebSocketMessage) => {
			this.notifyListeners('inventory_alert', data)
		})

		this.socket.on('scan_update', (data: WebSocketMessage) => {
			this.notifyListeners('scan_update', data)
		})
	}

	disconnect() {
		if (this.socket) {
			this.socket.disconnect()
			this.socket = null
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
		return this.socket?.connected || false
	}
}

export const wsService = new WebSocketService()

