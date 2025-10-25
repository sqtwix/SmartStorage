import { AIPredictionResponse, DashboardData } from '@/types'

import { apiClient } from './client'

export const dashboardApi = {
	getCurrentData: async (): Promise<DashboardData> => {
		const response = await apiClient.get<DashboardData>('/api/dashboard/current')
		return response.data
	},

	getAIPredictions: async (periodDays: number = 7, categories?: string[]): Promise<AIPredictionResponse> => {
		const response = await apiClient.post<AIPredictionResponse>('/api/ai/predict', {
			period_days: periodDays,
			categories: categories || [],
		})
		return response.data
	},
}

