import { useState } from 'react'
import { Home, Calendar, CalendarDays, BarChart3, Upload, Settings, ChevronDown, LogOut } from 'lucide-react'
import HomePage from './pages/HomePage'
import Dashboard from './pages/Dashboard'
import TimesheetPage from './pages/TimesheetPage'
import SettingsPage from './pages/SettingsPage'
import LoginPage from './pages/LoginPage'
import { useAuth } from './auth/AuthContext'

const NAV_ITEMS = [
  { id: 'home', icon: Home, label: 'Home' },
  { id: 'timesheet', icon: Calendar, label: 'My Timesheet' },
  { id: 'calendar', icon: CalendarDays, label: 'Calendar View' },
  { id: 'reports', icon: BarChart3, label: 'Reports' },
  { id: 'import', icon: Upload, label: 'Import Data' },
  { id: 'settings', icon: Settings, label: 'Settings' }
]

function initialsOf(name: string) {
  return name.split(' ').map(part => part[0]).join('').slice(0, 2).toUpperCase()
}

export default function App() {
  const { isAuthenticated, isLoading, employee, logout } = useAuth()
  const [currentPage, setCurrentPage] = useState('home')
  const [menuOpen, setMenuOpen] = useState(false)

  if (isLoading) {
    return <div className="h-screen flex items-center justify-center text-gray-500">Loading…</div>
  }

  if (!isAuthenticated || !employee) {
    return <LoginPage />
  }

  const pageTitle = NAV_ITEMS.find(item => item.id === currentPage)?.label ?? 'TimeSmart'

  return (
    <div className="flex h-screen bg-gray-50">
      <aside className="w-64 bg-gradient-to-b from-indigo-950 to-indigo-900 text-white flex flex-col p-4">
        <div className="flex items-center gap-3 mb-8">
          <div className="w-10 h-10 bg-gradient-to-br from-indigo-400 to-purple-500 rounded-xl flex items-center justify-center font-bold text-lg">G</div>
          <div>
            <h1 className="text-lg font-bold leading-tight">TimeSmart</h1>
            <p className="text-xs text-indigo-300 leading-tight">Work Simplified</p>
          </div>
        </div>

        <nav className="space-y-1 flex-1">
          {NAV_ITEMS.map(item => (
            <button
              key={item.id}
              onClick={() => setCurrentPage(item.id)}
              className={`w-full flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-all ${
                currentPage === item.id
                  ? 'bg-white text-indigo-900'
                  : 'text-indigo-100 hover:bg-indigo-800'
              }`}
            >
              <item.icon size={18} />
              <span>{item.label}</span>
            </button>
          ))}
        </nav>

        <div className="text-xs text-indigo-300 italic border-t border-indigo-800 pt-4">
          "Automating today for a simpler tomorrow."
        </div>
      </aside>

      <main className="flex-1 overflow-auto">
        <header className="bg-white border-b border-gray-200 px-6 py-3 flex items-center justify-between">
          <h2 className="text-xl font-bold text-gray-800">{pageTitle}</h2>
          <div className="flex items-center gap-4">
            <button className="flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50">
              <Calendar size={16} />
              September 2025
              <ChevronDown size={16} />
            </button>
            <div className="relative">
              <button onClick={() => setMenuOpen(v => !v)} className="flex items-center gap-3">
                <div className="w-9 h-9 rounded-full bg-indigo-600 text-white flex items-center justify-center text-sm font-semibold">
                  {initialsOf(employee.displayName)}
                </div>
                <div className="text-sm text-left">
                  <p className="font-semibold text-gray-800 leading-tight">{employee.displayName}</p>
                  <p className="text-gray-500 leading-tight">Employee</p>
                </div>
                <ChevronDown size={16} className="text-gray-400" />
              </button>
              {menuOpen && (
                <div className="absolute right-0 mt-2 w-40 bg-white border border-gray-200 rounded-lg shadow-lg py-1 z-10">
                  <button
                    onClick={logout}
                    className="w-full flex items-center gap-2 px-4 py-2 text-sm text-red-600 hover:bg-red-50"
                  >
                    <LogOut size={16} /> Log out
                  </button>
                </div>
              )}
            </div>
          </div>
        </header>

        <div className="p-6">
          {currentPage === 'home' && <HomePage />}
          {currentPage === 'timesheet' && <TimesheetPage />}
          {currentPage === 'import' && <Dashboard />}
          {currentPage === 'settings' && <SettingsPage />}
        </div>
      </main>
    </div>
  )
}
