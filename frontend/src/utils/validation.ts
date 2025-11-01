export const validateEmail = (email: string): boolean => {
	// const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
	// return emailRegex.test(email)
	return email !== ''
}

export const validatePassword = (password: string): boolean => {
	// return password.length >= 8
	return password !== ''
}

export const getValidationError = (field: 'email' | 'password', value: string): string | null => {
	switch (field) {
		case 'email':
			if (!value) return 'Email обязателен'
			if (!validateEmail(value)) return 'Некорректный формат email'
			return null
		case 'password':
			if (!value) return 'Пароль обязателен'
			if (!validatePassword(value)) return 'Пароль должен содержать минимум 8 символов'
			return null
		default:
			return null
	}
}

