import { DashboardData, InventoryScan, Robot } from '@/types'

export const processDashboardData = (data: DashboardData): DashboardData => {
	return {
		...data,
		robots: data.robots.map(processRobot),
		recentScans: data.recentScans.map(processScan).sort((a, b) => {
			return new Date(b.scanned_at).getTime() - new Date(a.scanned_at).getTime()
		}),
	}
}

export const processRobot = (robot: Robot): Robot => {
	let status: Robot['status'] = 'active'

	if (robot.status === 'offline') {
		status = 'offline'
	} else if (robot.batteryLevel < 20) {
		status = 'low_battery'
	} else if (robot.status === 'idle') {
		status = 'idle'
	} else {
		status = 'active'
	}

	return {
		...robot,
		status,
	}
}

export const processScan = (scan: InventoryScan): InventoryScan => {
	return {
		...scan,
		status: determineStatus(scan.quantity),
	}
}

export const determineStatus = (quantity: number): 'OK' | 'LOW_STOCK' | 'CRITICAL' => {
	if (quantity <= 10) return 'CRITICAL'
	if (quantity <= 20) return 'LOW_STOCK'
	return 'OK'
}

export const getStatusColor = (status: 'OK' | 'LOW_STOCK' | 'CRITICAL'): string => {
	switch (status) {
	case 'OK':
		return '#4caf50'
	case 'LOW_STOCK':
		return '#ff9800'
	case 'CRITICAL':
		return '#f44336'
	}
}

export const getRobotStatusColor = (status: Robot['status']): string => {
	switch (status) {
	case 'active':
		return '#4caf50'
	case 'low_battery':
		return '#ff9800'
	case 'offline':
		return '#f44336'
	case 'idle':
		return '#9e9e9e'
	default:
		return '#9e9e9e'
	}
}

export const calculateAverageBatteryLevel = (robots: Robot[]): number => {
	if (robots.length === 0) return 0
	
	const totalBattery = robots.reduce((sum, robot) => sum + robot.batteryLevel, 0)
	return Math.round(totalBattery / robots.length)
}

