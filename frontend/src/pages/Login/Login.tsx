import { useState } from 'react'
import { useNavigate } from 'react-router-dom'

import { authApi } from '@/api'
import { Alert, Button, Input } from '@/shared'
import { getValidationError } from '@/utils'

import './Login.css'

export const Login = () => {
	const navigate = useNavigate()
	const [email, setEmail] = useState('')
	const [password, setPassword] = useState('')
	const [rememberMe, setRememberMe] = useState(false)
	const [loading, setLoading] = useState(false)
	const [error, setError] = useState<string | null>(null)
	const [fieldErrors, setFieldErrors] = useState<{ email?: string; password?: string }>({})

	const handleSubmit = async (e: React.FormEvent) => {
		e.preventDefault()

		const emailError = getValidationError('email', email)
		const passwordError = getValidationError('password', password)

		if (emailError || passwordError) {
			setFieldErrors({
				email: emailError || undefined,
				password: passwordError || undefined,
			})
			return
		}

		setFieldErrors({})
		setError(null)
		setLoading(true)

		try {
			const response = await authApi.login({ email, password })
			authApi.saveAuth(response.token, response.user)

			if (rememberMe) {
				localStorage.setItem('remember_me', 'true')
			}

			navigate('/dashboard')
		} catch (err) {
			setError('Неверный email или пароль')
		} finally {
			setLoading(false)
		}
	}

	return (
		<div className="login-page">
			<div className="login-container">
				<div className="login-logo">
					<img src="/rostelekom-logo.svg" alt="Ростелеком" className="logo-image" />
				</div>

				<div className="login-card">
					<h1 className="login-title">Умный склад</h1>
					<p className="login-subtitle">Войдите в систему для продолжения</p>

					{error && (
						<div className="login-error">
							<Alert type="error" message={error} onClose={() => setError(null)} />
						</div>
					)}

					<form onSubmit={handleSubmit} className="login-form">
						<Input
							type="email"
							placeholder="Введите email"
							value={email}
							onChange={(e) => setEmail(e.target.value)}
							error={fieldErrors.email}
							autoComplete="email"
						/>

						<Input
							type="password"
							placeholder="Введите пароль"
							value={password}
							onChange={(e) => setPassword(e.target.value)}
							error={fieldErrors.password}
							// autoComplete="current-password"
						/>

						<div className="login-checkbox">
							<label>
								<input type="checkbox" checked={rememberMe} onChange={(e) => setRememberMe(e.target.checked)} />
								<span>Запомнить меня</span>
							</label>
						</div>

						<Button type="submit" fullWidth loading={loading}>
							Войти
						</Button>

						<a href="#" className="login-forgot">
							Забыли пароль?
						</a>
					</form>
				</div>

				<div className="login-footer">
					<p>© 2024 Ростелеком. Технологии возможностей</p>
				</div>
			</div>
		</div>
	)
}

