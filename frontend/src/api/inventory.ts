import { CSVUploadResponse, HistoryFilters, HistoryResponse } from '@/types'

import { apiClient } from './client'

export const inventoryApi = {
	getHistory: async (filters: HistoryFilters): Promise<HistoryResponse> => {
		const params = new URLSearchParams()

		if (filters.from) params.append('from', filters.from)
		if (filters.to) params.append('to', filters.to)
		if (filters.zones?.length) params.append('zone', filters.zones.join(','))
		if (filters.status?.length) params.append('status', filters.status.join(','))
		if (filters.search) params.append('search', filters.search)
		if (filters.page) params.append('page', filters.page.toString())
		if (filters.limit) params.append('limit', filters.limit.toString())

		const response = await apiClient.get<HistoryResponse>(`/api/inventory/history?${params.toString()}`)
		return response.data
	},

	uploadCSV: async (file: File): Promise<CSVUploadResponse> => {
		const formData = new FormData()
		formData.append('file', file)

		const response = await apiClient.post<CSVUploadResponse>('/api/inventory/import', formData, {
			headers: {
				'Content-Type': 'multipart/form-data',
			},
		})
		return response.data
	},

	exportExcel: async (ids: number[]): Promise<Blob> => {
		const response = await apiClient.get(`/api/export/excel?ids=${ids.join(',')}`, {
			responseType: 'blob',
		})
		return response.data
	},
}

