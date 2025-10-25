import { LoginRequest, LoginResponse } from '@/types'

// import { apiClient } from './client' // TODO: раскомментировать когда будет готов бэкенд

export const authApi = {
	login: async (credentials: LoginRequest): Promise<LoginResponse> => {
		// TODO: когда будет готов бэкенд, раскомментировать реальный запрос
		// const response = await apiClient.post<LoginResponse>('/api/auth/login', credentials)
		// return response.data

		// Мок данных для тестирования
		return new Promise((resolve) => {
			setTimeout(() => {
				// Имитация успешного входа (можно использовать любой email/пароль)
				resolve({
					token: 'mock_jwt_token_1234567890',
					user: {
						id: 1,
						name: 'Иван Петров',
						email: credentials.email,
						role: 'operator',
					},
				})
			}, 800) // Имитация задержки сети
		})
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

