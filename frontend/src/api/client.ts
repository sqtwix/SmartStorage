import axios, { AxiosInstance, InternalAxiosRequestConfig } from 'axios'

class ApiClient {
	private client: AxiosInstance

	constructor() {
		this.client = axios.create({
			baseURL: '', // Пустой baseURL для работы с Vite proxy
			headers: {
				'Content-Type': 'application/json',
			},
		})

		this.client.interceptors.request.use(
			(config: InternalAxiosRequestConfig) => {
				const token = localStorage.getItem('auth_token')
				if (token && config.headers) {
					config.headers.Authorization = `Bearer ${token}`
				}
				return config
			},
			(error) => {
				return Promise.reject(error)
			}
		)

		this.client.interceptors.response.use(
			(response) => response,
			(error) => {
				if (error.response?.status === 401) {
					localStorage.removeItem('auth_token')
					localStorage.removeItem('user')
					window.location.href = '/login'
				}
				return Promise.reject(error)
			}
		)
	}

	getClient(): AxiosInstance {
		return this.client
	}
}

export const apiClient = new ApiClient().getClient()

