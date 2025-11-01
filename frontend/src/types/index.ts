// Типы для пользователя и авторизации
export interface User {
	id: number
	name: string
	email: string
	role: 'operator' | 'admin' | 'viewer'
}

export interface LoginRequest {
	email: string
	password: string
}

export interface LoginResponse {
	token: string
	user: User
}

// Типы для роботов
export interface Robot {
	id: string
	status: 'active' | 'low_battery' | 'offline' | 'idle'
	batteryLevel: number
	lastUpdate: string
	currentZone: string
	currentRow: number
	currentShelf: number
}

export interface RobotLocation {
	zone: string
	row: number
	shelf: number
}

// Типы для товаров
export interface Product {
	id: string
	name: string
	category?: string
	min_stock: number
	optimal_stock: number
}

// Типы для сканирований
export interface ScanResult {
	product_id: string
	product_name: string
	quantity: number
	status: 'OK' | 'LOW_STOCK' | 'CRITICAL'
}

export interface InventoryScan {
	id: number
	robot_id: string
	product_id: string
	product_name: string
	quantity: number
	zone: string
	row_number?: number
	shelf_number?: number
	status: 'OK' | 'LOW_STOCK' | 'CRITICAL'
	scanned_at: string
	created_at: string
}

// Типы для дашборда
export interface DashboardStatistics {
	total_products: number
	total_scans: number
	critical_products: number
	active_robots: number
	last_update: string
}

export interface DashboardData {
	robots: Robot[]
	recentScans: InventoryScan[]
	stats: DashboardStatistics
}

// Типы для исторических данных
export interface HistoryFilters {
	from?: string
	to?: string
	zones?: string[]
	categories?: string[]
	status?: ('OK' | 'LOW_STOCK' | 'CRITICAL')[]
	search?: string
	page?: number
	limit?: number
}

export interface HistoryRecord {
	id: number
	date: string
	robot_id: string
	zone: string
	product_id: string
	product_name: string
	expected_quantity: number
	actual_quantity: number
	difference: number
	status: 'OK' | 'LOW_STOCK' | 'CRITICAL'
}

export interface HistoryResponse {
	total: number
	items: HistoryRecord[]
	pagination: {
		page: number
		limit: number
		total_pages: number
	}
}

// Типы для прогнозов ИИ
export interface AIPrediction {
	product_id: string
	product_name: string
	current_stock: number
	days_until_stockout: number
	recommended_order: number
	prediction_date: string
}

export interface AIPredictionResponse {
	predictions: AIPrediction[]
	confidence: number
}

// Типы для WebSocket
export type WebSocketMessageType = 'robot_update' | 'inventory_alert' | 'scan_update'

export interface WebSocketMessage {
	type: WebSocketMessageType
	data: Robot | InventoryScan | { message: string; severity: 'info' | 'warning' | 'error' }
}

// Типы для загрузки CSV
export interface CSVUploadResponse {
	success: number
	failed: number
	errors: string[]
}

export interface CSVRow {
	product_id: string
	product_name: string
	quantity: number
	zone: string
	date: string
	row: number
	shelf: number
}

