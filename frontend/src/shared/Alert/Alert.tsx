import './Alert.css'

interface AlertProps {
	type: 'success' | 'error' | 'warning' | 'info'
	message: string
	onClose?: () => void
}

export const Alert = ({ type, message, onClose }: AlertProps) => {
	return (
		<div className={`alert alert-${type}`}>
			<span className="alert-message">{message}</span>
			{onClose && (
				<button className="alert-close" onClick={onClose}>
					×
				</button>
			)}
		</div>
	)
}

