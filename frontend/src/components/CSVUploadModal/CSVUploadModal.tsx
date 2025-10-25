import { useState } from 'react'

import { inventoryApi } from '@/api'
import { parseCSVPreview } from '@/data-layer'
import { Alert, Button, Modal } from '@/shared'

import './CSVUploadModal.css'

interface CSVUploadModalProps {
	isOpen: boolean
	onClose: () => void
	onSuccess: () => void
}

export const CSVUploadModal = ({ isOpen, onClose, onSuccess }: CSVUploadModalProps) => {
	const [file, setFile] = useState<File | null>(null)
	const [preview, setPreview] = useState<string[][] | null>(null)
	const [uploading, setUploading] = useState(false)
	const [error, setError] = useState<string | null>(null)
	const [success, setSuccess] = useState<string | null>(null)

	const handleFileChange = (selectedFile: File | null) => {
		if (!selectedFile) return

		setFile(selectedFile)
		setError(null)

		const reader = new FileReader()
		reader.onload = (e) => {
			const text = e.target?.result as string
			const previewData = parseCSVPreview(text, ';')
			setPreview(previewData)
		}
		reader.readAsText(selectedFile, 'UTF-8')
	}

	const handleDrop = (e: React.DragEvent) => {
		e.preventDefault()
		const droppedFile = e.dataTransfer.files[0]
		if (droppedFile && droppedFile.type === 'text/csv') {
			handleFileChange(droppedFile)
		} else {
			setError('Пожалуйста, загрузите файл формата CSV')
		}
	}

	const handleUpload = async () => {
		if (!file) return

		setUploading(true)
		setError(null)

		try {
			const result = await inventoryApi.uploadCSV(file)
			setSuccess(`Успешно загружено: ${result.success} записей. Ошибок: ${result.failed}`)
			setTimeout(() => {
				onSuccess()
				handleClose()
			}, 2000)
		} catch (err) {
			setError('Ошибка при загрузке файла')
		} finally {
			setUploading(false)
		}
	}

	const handleClose = () => {
		setFile(null)
		setPreview(null)
		setError(null)
		setSuccess(null)
		onClose()
	}

	return (
		<Modal
			isOpen={isOpen}
			onClose={handleClose}
			title="Загрузка данных инвентаризации"
			footer={
				<>
					<Button variant="secondary" onClick={handleClose} disabled={uploading}>
						Отмена
					</Button>
					<Button variant="primary" onClick={handleUpload} disabled={!file || uploading} loading={uploading}>
						Загрузить
					</Button>
				</>
			}
		>
			<div className="csv-upload-content">
				{error && <Alert type="error" message={error} onClose={() => setError(null)} />}
				{success && <Alert type="success" message={success} />}

				<div className="csv-drop-zone" onDrop={handleDrop} onDragOver={(e) => e.preventDefault()}>
					<div className="drop-zone-icon">📁</div>
					<p className="drop-zone-text">Перетащите CSV файл сюда или нажмите для выбора</p>
					<input type="file" accept=".csv" onChange={(e) => handleFileChange(e.target.files?.[0] || null)} className="file-input" />
				</div>

				<div className="csv-requirements">
					<p className="requirements-title">Требования к файлу:</p>
					<ul>
						<li>Формат: CSV с разделителем ";"</li>
						<li>Кодировка: UTF-8</li>
						<li>Обязательные колонки: product_id, product_name, quantity, zone, date</li>
					</ul>
				</div>

				{file && <div className="selected-file">Выбран файл: {file.name}</div>}

				{preview && (
					<div className="csv-preview">
						<p className="preview-title">Предпросмотр (первые 5 строк):</p>
						<div className="preview-table-container">
							<table className="preview-table">
								<tbody>
									{preview.map((row, idx) => (
										<tr key={idx}>
											{row.map((cell, cellIdx) => (
												<td key={cellIdx}>{cell}</td>
											))}
										</tr>
									))}
								</tbody>
							</table>
						</div>
					</div>
				)}
			</div>
		</Modal>
	)
}

