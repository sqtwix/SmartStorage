import './Button.css'

interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
	variant?: 'primary' | 'secondary' | 'danger'
	fullWidth?: boolean
	loading?: boolean
}

export const Button = ({ variant = 'primary', fullWidth = false, loading = false, children, ...props }: ButtonProps) => {
	return (
		<button className={`btn btn-${variant} ${fullWidth ? 'btn-full-width' : ''}`} disabled={loading || props.disabled} {...props}>
			{loading ? <span className="btn-loader"></span> : children}
		</button>
	)
}

