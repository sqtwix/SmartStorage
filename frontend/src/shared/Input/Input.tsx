import './Input.css'

interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
	label?: string
	error?: string
}

export const Input = ({ label, error, ...props }: InputProps) => {
	return (
		<div className="input-wrapper">
			{label && <label className="input-label">{label}</label>}
			<input className={`input ${error ? 'input-error' : ''}`} {...props} />
			{error && <span className="input-error-text">{error}</span>}
		</div>
	)
}

