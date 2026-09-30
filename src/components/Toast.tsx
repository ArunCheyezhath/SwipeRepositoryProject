import { AlertCircle, CheckCircle, Info, X } from 'lucide-react'
import { useEffect, useState } from 'react'

type ToastType = 'success' | 'error' | 'info' | 'warning'

interface ToastProps {
  message: string
  type?: ToastType
  duration?: number
  onClose?: () => void
}

export function Toast({ message, type = 'info', duration = 5000, onClose }: ToastProps) {
  const [isVisible, setIsVisible] = useState(true)

  useEffect(() => {
    const timer = setTimeout(() => {
      setIsVisible(false)
      onClose?.()
    }, duration)
    return () => clearTimeout(timer)
  }, [duration, onClose])

  if (!isVisible) return null

  const styles = {
    success: 'bg-green-50 border-green-200 text-green-800',
    error: 'bg-red-50 border-red-200 text-red-800',
    info: 'bg-blue-50 border-blue-200 text-blue-800',
    warning: 'bg-yellow-50 border-yellow-200 text-yellow-800'
  }

  const icons = {
    success: <CheckCircle size={20} />,
    error: <AlertCircle size={20} />,
    info: <Info size={20} />,
    warning: <AlertCircle size={20} />
  }

  return (
    <div className={`fixed top-4 right-4 max-w-sm p-4 rounded-lg border ${styles[type]} shadow-lg flex items-center gap-3 animate-in fade-in slide-in-from-top-2 z-50`}>
      {icons[type]}
      <span className="flex-1">{message}</span>
      <button
        onClick={() => {
          setIsVisible(false)
          onClose?.()
        }}
        className="hover:opacity-70"
      >
        <X size={16} />
      </button>
    </div>
  )
}

export function useToast() {
  const [toast, setToast] = useState<ToastProps | null>(null)

  const show = (message: string, type: ToastType = 'info', duration = 5000) => {
    setToast({ message, type, duration })
  }

  return { toast, show, clear: () => setToast(null) }
}
