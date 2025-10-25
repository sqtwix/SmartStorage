import { format, parseISO } from 'date-fns'
import { ru } from 'date-fns/locale'

export const formatDateTime = (date: string | Date): string => {
	try {
		const dateObj = typeof date === 'string' ? parseISO(date) : date
		return format(dateObj, 'dd.MM.yyyy HH:mm:ss', { locale: ru })
	} catch {
		return 'Некорректная дата'
	}
}

export const formatDate = (date: string | Date): string => {
	try {
		const dateObj = typeof date === 'string' ? parseISO(date) : date
		return format(dateObj, 'dd.MM.yyyy', { locale: ru })
	} catch {
		return 'Некорректная дата'
	}
}

export const formatTime = (date: string | Date): string => {
	try {
		const dateObj = typeof date === 'string' ? parseISO(date) : date
		return format(dateObj, 'HH:mm:ss', { locale: ru })
	} catch {
		return 'Некорректное время'
	}
}

export const formatNumber = (num: number): string => {
	return new Intl.NumberFormat('ru-RU').format(num)
}

export const formatPercent = (num: number): string => {
	return `${num.toFixed(1)}%`
}

