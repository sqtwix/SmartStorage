import { HistoryRecord } from '@/types'

export const processHistoryRecord = (record: HistoryRecord): HistoryRecord => {
	return {
		...record,
		difference: record.actual_quantity - record.expected_quantity,
	}
}

export const processHistoryRecords = (records: HistoryRecord[]): HistoryRecord[] => {
	return records.map(processHistoryRecord)
}

export const downloadBlob = (blob: Blob, filename: string) => {
	const url = window.URL.createObjectURL(blob)
	const link = document.createElement('a')
	link.href = url
	link.download = filename
	document.body.appendChild(link)
	link.click()
	document.body.removeChild(link)
	window.URL.revokeObjectURL(url)
}

export const parseCSVPreview = (csvText: string, delimiter: string = ';'): string[][] => {
	const lines = csvText.split('\n').filter((line) => line.trim())
	return lines.slice(0, 6).map((line) => line.split(delimiter))
}

