import { LoginRequest, LoginResponse } from '@/types'

import { apiClient } from './client'

export const authApi = {
	login: async (credentials: LoginRequest): Promise<LoginResponse> => {
		console.log('login', credentials)
		const response = await apiClient.post<LoginResponse>('/api/auth/login', credentials)
		return response.data
	},

	logout: () => {
		localStorage.removeItem('auth_token')
		localStorage.removeItem('user')
	},

	getStoredUser: () => {
		const userStr = localStorage.getItem('user')
		return userStr ? JSON.parse(userStr) : null
	},

	getStoredToken: () => {
		return localStorage.getItem('auth_token')
	},

	saveAuth: (token: string, user: LoginResponse['user']) => {
		localStorage.setItem('auth_token', token)
		localStorage.setItem('user', JSON.stringify(user))
	},
}

