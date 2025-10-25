import './Loader.css'

interface LoaderProps {
	size?: 'small' | 'medium' | 'large'
	fullScreen?: boolean
}

export const Loader = ({ size = 'medium', fullScreen = false }: LoaderProps) => {
	if (fullScreen) {
		return (
			<div className="loader-fullscreen">
				<div className={`loader loader-${size}`}></div>
			</div>
		)
	}

	return <div className={`loader loader-${size}`}></div>
}

