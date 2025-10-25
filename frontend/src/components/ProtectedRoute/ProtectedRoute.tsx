import { Navigate } from 'react-router-dom'

import { authApi } from '@/api'

interface ProtectedRouteProps {
	children: React.ReactNode
}

export const ProtectedRoute = ({ children }: ProtectedRouteProps) => {
	const token = authApi.getStoredToken()

	if (!token) {
		return <Navigate to="/login" replace />
	}

	return <>{children}</>
}

