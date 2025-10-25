import { CSVUploadResponse, HistoryFilters, HistoryResponse } from '@/types'

// import { apiClient } from './client' // TODO: раскомментировать когда будет готов бэкенд

export const inventoryApi = {
	getHistory: async (filters: HistoryFilters): Promise<HistoryResponse> => {
		// TODO: когда будет готов бэкенд, раскомментировать реальный запрос
		// const params = new URLSearchParams()
		// if (filters.from) params.append('from', filters.from)
		// if (filters.to) params.append('to', filters.to)
		// if (filters.zones?.length) params.append('zone', filters.zones.join(','))
		// if (filters.status?.length) params.append('status', filters.status.join(','))
		// if (filters.search) params.append('search', filters.search)
		// if (filters.page) params.append('page', filters.page.toString())
		// if (filters.limit) params.append('limit', filters.limit.toString())
		// const response = await apiClient.get<HistoryResponse>(`/api/inventory/history?${params.toString()}`)
		// return response.data

		// Мок данных для тестирования
		return new Promise((resolve) => {
			setTimeout(() => {
				const page = filters.page || 1
				const limit = filters.limit || 20

				// Генерируем тестовые данные
				const mockData = []
				const products = [
					{ id: 'TEL-4567', name: 'Роутер RT-AC68U' },
					{ id: 'TEL-8901', name: 'Модем DSL-2640U' },
					{ id: 'TEL-2345', name: 'Коммутатор SG-108' },
					{ id: 'TEL-6789', name: 'IP-телефон T46S' },
					{ id: 'TEL-3456', name: 'Кабель UTP Cat6' },
					{ id: 'TEL-9876', name: 'Адаптер питания 12V' },
					{ id: 'TEL-5432', name: 'Антенна Wi-Fi 5dBi' },
					{ id: 'TEL-7890', name: 'Патч-корд 3м' },
				]
				const zones = ['A', 'B', 'C', 'D', 'E']
				const robots = ['RB-001', 'RB-002', 'RB-003', 'RB-004', 'RB-005']

				for (let i = 0; i < 100; i++) {
					const product = products[i % products.length]
					const expected = Math.floor(Math.random() * 100) + 20
					const actual = expected + Math.floor(Math.random() * 21) - 10
					const status: 'OK' | 'LOW_STOCK' | 'CRITICAL' = actual <= 10 ? 'CRITICAL' : actual <= 20 ? 'LOW_STOCK' : 'OK'

					mockData.push({
						id: i + 1,
						date: new Date(Date.now() - i * 3600000).toISOString(),
						robot_id: robots[i % robots.length],
						zone: zones[i % zones.length],
						product_id: product.id,
						product_name: product.name,
						expected_quantity: expected,
						actual_quantity: actual,
						difference: actual - expected,
						status: status,
					})
				}

				// Применяем фильтры
				let filtered = mockData

				if (filters.zones?.length) {
					filtered = filtered.filter((item) => filters.zones!.includes(item.zone))
				}

				if (filters.status?.length) {
					filtered = filtered.filter((item) => filters.status!.includes(item.status as 'OK' | 'LOW_STOCK' | 'CRITICAL'))
				}

				if (filters.search) {
					const search = filters.search.toLowerCase()
					filtered = filtered.filter(
						(item) => item.product_id.toLowerCase().includes(search) || item.product_name.toLowerCase().includes(search)
					)
				}

				const total = filtered.length
				const start = (page - 1) * limit
				const end = start + limit
				const items = filtered.slice(start, end)

				resolve({
					total,
					items,
					pagination: {
						page,
						limit,
						total_pages: Math.ceil(total / limit),
					},
				})
			}, 600) // Имитация задержки сети
		})
	},

	uploadCSV: async (_file: File): Promise<CSVUploadResponse> => {
		// TODO: когда будет готов бэкенд, раскомментировать реальный запрос
		// const formData = new FormData()
		// formData.append('file', _file)
		// const response = await apiClient.post<CSVUploadResponse>('/api/inventory/import', formData, {
		// 	headers: {
		// 		'Content-Type': 'multipart/form-data',
		// 	},
		// })
		// return response.data

		// Мок данных для тестирования
		return new Promise((resolve) => {
			setTimeout(() => {
				// Имитируем успешную загрузку
				resolve({
					success: 156,
					failed: 3,
					errors: [
						'Строка 45: неверный формат даты',
						'Строка 78: отсутствует обязательное поле product_id',
						'Строка 92: некорректное значение quantity',
					],
				})
			}, 1500) // Имитация задержки загрузки файла
		})
	},

	exportExcel: async (ids: number[]): Promise<Blob> => {
		// TODO: когда будет готов бэкенд, раскомментировать реальный запрос
		// const response = await apiClient.get(`/api/export/excel?ids=${ids.join(',')}`, {
		// 	responseType: 'blob',
		// })
		// return response.data

		// Мок данных для тестирования
		return new Promise((resolve) => {
			setTimeout(() => {
				// Создаем простой blob для имитации Excel файла
				const content = `ID,Date,Robot,Zone,Product,Expected,Actual,Difference,Status
${ids.map((id) => `${id},2024-10-25,RB-001,A,TEL-4567,50,48,-2,OK`).join('\n')}`

				const blob = new Blob([content], { type: 'application/vnd.ms-excel' })
				resolve(blob)
			}, 800) // Имитация задержки экспорта
		})
	},
}

