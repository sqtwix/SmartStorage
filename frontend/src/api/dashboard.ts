import { AIPredictionResponse, DashboardData } from '@/types'

import { apiClient } from './client'

export const dashboardApi = {
	getCurrentData: async (): Promise<DashboardData> => {
		const response = await apiClient.get<DashboardData>('/api/dashboard/current')
		return response.data
	},

	getAIPredictions: async (_periodDays: number = 7, _categories?: string[]): Promise<AIPredictionResponse> => {
		const response = await apiClient.post<AIPredictionResponse>('/api/ai/predict', {
			period_days: _periodDays,
			categories: _categories || [],
		})
		return response.data
	},
}

