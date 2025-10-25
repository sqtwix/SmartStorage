import { AIPredictionResponse, DashboardData } from '@/types'

// import { apiClient } from './client' // TODO: раскомментировать когда будет готов бэкенд

export const dashboardApi = {
	getCurrentData: async (): Promise<DashboardData> => {
		// TODO: когда будет готов бэкенд, раскомментировать реальный запрос
		// const response = await apiClient.get<DashboardData>('/api/dashboard/current')
		// return response.data

		// Мок данных для тестирования
		return new Promise((resolve) => {
			setTimeout(() => {
				resolve({
					robots: [
						{
							id: 'RB-001',
							status: 'active',
							battery_level: 85,
							last_update: new Date().toISOString(),
							current_zone: 'A',
							current_row: 5,
							current_shelf: 3,
						},
						{
							id: 'RB-002',
							status: 'active',
							battery_level: 92,
							last_update: new Date().toISOString(),
							current_zone: 'B',
							current_row: 8,
							current_shelf: 7,
						},
						{
							id: 'RB-003',
							status: 'low_battery',
							battery_level: 18,
							last_update: new Date().toISOString(),
							current_zone: 'C',
							current_row: 12,
							current_shelf: 2,
						},
						{
							id: 'RB-004',
							status: 'active',
							battery_level: 76,
							last_update: new Date().toISOString(),
							current_zone: 'D',
							current_row: 3,
							current_shelf: 9,
						},
						{
							id: 'RB-005',
							status: 'offline',
							battery_level: 5,
							last_update: new Date(Date.now() - 3600000).toISOString(),
							current_zone: 'E',
							current_row: 15,
							current_shelf: 4,
						},
					],
					recent_scans: [
						{
							id: 1,
							robot_id: 'RB-001',
							product_id: 'TEL-4567',
							product_name: 'Роутер RT-AC68U',
							quantity: 45,
							zone: 'A',
							row_number: 5,
							shelf_number: 3,
							status: 'OK',
							scanned_at: new Date(Date.now() - 120000).toISOString(),
							created_at: new Date(Date.now() - 120000).toISOString(),
						},
						{
							id: 2,
							robot_id: 'RB-002',
							product_id: 'TEL-8901',
							product_name: 'Модем DSL-2640U',
							quantity: 12,
							zone: 'B',
							row_number: 8,
							shelf_number: 7,
							status: 'LOW_STOCK',
							scanned_at: new Date(Date.now() - 180000).toISOString(),
							created_at: new Date(Date.now() - 180000).toISOString(),
						},
						{
							id: 3,
							robot_id: 'RB-004',
							product_id: 'TEL-2345',
							product_name: 'Коммутатор SG-108',
							quantity: 8,
							zone: 'D',
							row_number: 3,
							shelf_number: 9,
							status: 'CRITICAL',
							scanned_at: new Date(Date.now() - 240000).toISOString(),
							created_at: new Date(Date.now() - 240000).toISOString(),
						},
						{
							id: 4,
							robot_id: 'RB-001',
							product_id: 'TEL-6789',
							product_name: 'IP-телефон T46S',
							quantity: 67,
							zone: 'A',
							row_number: 7,
							shelf_number: 2,
							status: 'OK',
							scanned_at: new Date(Date.now() - 300000).toISOString(),
							created_at: new Date(Date.now() - 300000).toISOString(),
						},
						{
							id: 5,
							robot_id: 'RB-002',
							product_id: 'TEL-3456',
							product_name: 'Кабель UTP Cat6',
							quantity: 156,
							zone: 'B',
							row_number: 10,
							shelf_number: 5,
							status: 'OK',
							scanned_at: new Date(Date.now() - 360000).toISOString(),
							created_at: new Date(Date.now() - 360000).toISOString(),
						},
					],
					statistics: {
						active_robots: 3,
						total_robots: 5,
						checked_today: 1247,
						critical_items: 8,
						average_battery: 55.2,
					},
				})
			}, 500) // Имитация задержки сети
		})
	},

	getAIPredictions: async (_periodDays: number = 7, _categories?: string[]): Promise<AIPredictionResponse> => {
		// TODO: когда будет готов бэкенд, раскомментировать реальный запрос
		// const response = await apiClient.post<AIPredictionResponse>('/api/ai/predict', {
		// 	period_days: _periodDays,
		// 	categories: _categories || [],
		// })
		// return response.data

		// Мок данных для тестирования
		return new Promise((resolve) => {
			setTimeout(() => {
				resolve({
					predictions: [
						{
							product_id: 'TEL-2345',
							product_name: 'Коммутатор SG-108',
							current_stock: 8,
							days_until_stockout: 3,
							recommended_order: 50,
							prediction_date: new Date().toISOString(),
						},
						{
							product_id: 'TEL-8901',
							product_name: 'Модем DSL-2640U',
							current_stock: 12,
							days_until_stockout: 5,
							recommended_order: 40,
							prediction_date: new Date().toISOString(),
						},
						{
							product_id: 'TEL-9876',
							product_name: 'Адаптер питания 12V',
							current_stock: 15,
							days_until_stockout: 6,
							recommended_order: 60,
							prediction_date: new Date().toISOString(),
						},
						{
							product_id: 'TEL-5432',
							product_name: 'Антенна Wi-Fi 5dBi',
							current_stock: 9,
							days_until_stockout: 4,
							recommended_order: 35,
							prediction_date: new Date().toISOString(),
						},
						{
							product_id: 'TEL-7890',
							product_name: 'Патч-корд 3м',
							current_stock: 18,
							days_until_stockout: 7,
							recommended_order: 80,
							prediction_date: new Date().toISOString(),
						},
					],
					confidence: 87.5,
				})
			}, 800) // Имитация задержки сети для AI запроса
		})
	},
}

