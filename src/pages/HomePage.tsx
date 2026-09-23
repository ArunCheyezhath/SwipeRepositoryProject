import { useEffect, useMemo, useState } from 'react'
import { Building2, Home as HomeIcon, CalendarOff, Palmtree, Clock, Plus, RotateCw, Send, ChevronLeft, ChevronRight } from 'lucide-react'
import { API_ENDPOINTS } from '../config'
import { apiClient } from '../auth/apiClient'
import { useAuth } from '../auth/AuthContext'

const MONTH_START = '2025-09-01'
const MONTH_END = '2025-09-30'

type DayPeriod = 'morning' | 'afternoon' | 'evening' | 'night'

function getDayPeriod(hour: number): DayPeriod {
  if (hour >= 5 && hour < 12) return 'morning'
  if (hour >= 12 && hour < 17) return 'afternoon'
  if (hour >= 17 && hour < 21) return 'evening'
  return 'night'
}

const GREETING_LABEL: Record<DayPeriod, string> = {
  morning: 'Good Morning',
  afternoon: 'Good Afternoon',
  evening: 'Good Evening',
  night: 'Good Night'
}

const BANNER_THEME: Record<DayPeriod, { bg: string; heading: string; body: string; tagline: string }> = {
  morning: { bg: 'from-indigo-50 to-blue-50', heading: 'text-gray-900', body: 'text-gray-600', tagline: 'text-indigo-800' },
  afternoon: { bg: 'from-sky-50 to-cyan-50', heading: 'text-gray-900', body: 'text-gray-600', tagline: 'text-sky-800' },
  evening: { bg: 'from-orange-50 to-rose-100', heading: 'text-gray-900', body: 'text-gray-600', tagline: 'text-orange-800' },
  night: { bg: 'from-indigo-950 to-slate-900', heading: 'text-white', body: 'text-indigo-200', tagline: 'text-indigo-200' }
}

function SkylineIllustration({ period }: { period: DayPeriod }) {
  if (period === 'night') {
    return (
      <svg viewBox="0 0 220 110" className="hidden md:block absolute right-0 bottom-0 w-48 h-20 opacity-90 z-0">
        <circle cx="60" cy="14" r="1.5" fill="#E0E7FF" />
        <circle cx="90" cy="8" r="1" fill="#E0E7FF" />
        <circle cx="130" cy="12" r="1.2" fill="#E0E7FF" />
        <circle cx="175" cy="52" r="12" fill="#E0E7FF" />
        <path d="M0 90 L45 45 L75 70 L110 30 L150 65 L220 40 L220 110 L0 110 Z" fill="#3730A3" opacity="0.6" />
        <path d="M0 100 L60 60 L100 85 L140 55 L220 80 L220 110 L0 110 Z" fill="#312E81" opacity="0.8" />
      </svg>
    )
  }

  const sunColor = period === 'morning' ? '#FDE68A' : period === 'afternoon' ? '#FCD34D' : '#FB923C'
  const nearMountain = period === 'evening' ? '#FDBA74' : '#C7D2FE'
  const farMountain = period === 'evening' ? '#F97316' : '#A5B4FC'

  return (
    <svg viewBox="0 0 220 110" className="hidden md:block absolute right-0 bottom-0 w-48 h-20 opacity-90 z-0">
      <circle cx="175" cy="52" r="16" fill={sunColor} />
      <path d="M0 90 L45 45 L75 70 L110 30 L150 65 L220 40 L220 110 L0 110 Z" fill={nearMountain} opacity="0.7" />
      <path d="M0 100 L60 60 L100 85 L140 55 L220 80 L220 110 L0 110 Z" fill={farMountain} opacity="0.8" />
    </svg>
  )
}

const WORK_TYPE_STYLES: Record<string, string> = {
  Office: 'bg-blue-100 text-blue-700',
  WFH: 'bg-green-100 text-green-700',
  Leave: 'bg-purple-100 text-purple-700',
  Holiday: 'bg-pink-100 text-pink-700',
  Weekend: 'bg-gray-100 text-gray-600'
}

const WORK_TYPE_ICONS: Record<string, any> = {
  Office: Building2,
  WFH: HomeIcon,
  Leave: CalendarOff,
  Holiday: Palmtree,
  Weekend: Clock
}

function formatTime(time: string | null | undefined) {
  if (!time) return '–'
  const [h, m] = time.split(':')
  return `${h}:${m}`
}

function formatDate(isoDate: string) {
  return new Date(isoDate).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })
}

function formatHoursMinutes(hours: number) {
  const h = Math.floor(hours)
  const m = Math.round((hours - h) * 60)
  return `${h}h ${m.toString().padStart(2, '0')}m`
}

function chunkIntoWeeks(entries: any[]) {
  const weeks: any[][] = []
  let current: any[] = []
  entries.forEach((entry) => {
    const day = new Date(entry.date).getDay()
    current.push(entry)
    if (day === 0 || current.length === 7) {
      weeks.push(current)
      current = []
    }
  })
  if (current.length) weeks.push(current)
  return weeks
}

export default function HomePage() {
  const { employee } = useAuth()
  const employeeId = employee!.employeeId
  const [period, setPeriod] = useState<DayPeriod>(() => getDayPeriod(new Date().getHours()))

  useEffect(() => {
    const timer = setInterval(() => setPeriod(getDayPeriod(new Date().getHours())), 60_000)
    return () => clearInterval(timer)
  }, [])
  const [timesheet, setTimesheet] = useState<any>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [weekIndex, setWeekIndex] = useState(2)
  const [view, setView] = useState<'week' | 'month'>('week')

  const loadTimesheet = async () => {
    setLoading(true)
    setError('')
    try {
      const response = await apiClient.post(
        API_ENDPOINTS.GENERATE,
        null,
        { params: { employeeId, startDate: MONTH_START, endDate: MONTH_END } }
      )
      setTimesheet(response.data)
    } catch (err: any) {
      setError(err.message)
    }
    setLoading(false)
  }

  useEffect(() => {
    loadTimesheet()
  }, [])

  const handleRecalculate = async () => {
    try {
      const response = await apiClient.post(
        API_ENDPOINTS.RECALCULATE,
        null,
        { params: { employeeId, startDate: MONTH_START, endDate: MONTH_END } }
      )
      setTimesheet((prev: any) => ({ ...prev, entries: response.data }))
    } catch (err: any) {
      setError(err.message)
    }
  }

  const handleSubmit = async () => {
    if (!timesheet) return
    try {
      await apiClient.post(API_ENDPOINTS.SUBMIT, timesheet)
      alert('Timesheet submitted successfully!')
    } catch (err: any) {
      setError(err.message)
    }
  }

  const weeks = useMemo(() => chunkIntoWeeks(timesheet?.entries ?? []), [timesheet])
  const currentWeek = weeks[weekIndex] ?? []
  const weekTotal = currentWeek.reduce((sum: number, e: any) => sum + (e.hours ?? 0), 0)

  if (loading) {
    return <div className="max-w-6xl mx-auto py-20 text-center text-gray-500">Loading your timesheet…</div>
  }

  if (error) {
    return (
      <div className="max-w-6xl mx-auto py-20 text-center text-red-600">
        Couldn't load timesheet data: {error}
      </div>
    )
  }

  const stats = [
    { label: 'Office Days', value: timesheet.officeDays, sub: formatHoursMinutes(timesheet.officeHours), icon: Building2, color: 'bg-green-50 text-green-700' },
    { label: 'WFH Days', value: timesheet.wfhDays, sub: formatHoursMinutes(timesheet.wfhHours), icon: HomeIcon, color: 'bg-blue-50 text-blue-700' },
    { label: 'Leave Days', value: timesheet.leaveDays, sub: formatHoursMinutes(timesheet.leaveHours), icon: CalendarOff, color: 'bg-purple-50 text-purple-700' },
    { label: 'Holiday', value: timesheet.holidayDays, sub: formatHoursMinutes(timesheet.holidayHours), icon: Palmtree, color: 'bg-pink-50 text-pink-700' }
  ]

  const theme = BANNER_THEME[period]

  return (
    <div className="max-w-6xl mx-auto">
      <div className={`relative overflow-hidden rounded-2xl bg-gradient-to-br ${theme.bg} border border-indigo-100 p-8 mb-6`}>
        <div className="relative z-10 md:pr-40">
          <h1 className={`text-3xl font-bold mb-1 ${theme.heading}`}>{GREETING_LABEL[period]}, {employee!.displayName.split(' ')[0]}! {'\u{1F44B}'}</h1>
          <p className={theme.body}>Your timesheet is ready. Review, make changes if needed, and submit.</p>
        </div>
        <p className={`hidden md:block absolute top-6 right-8 font-serif italic text-lg leading-tight text-right ${theme.tagline}`}>
          Same Effort<br />Greater Clarity
        </p>
        <SkylineIllustration period={period} />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-5 gap-4 mb-6">
        {stats.map((stat) => (
          <div key={stat.label} className={`rounded-2xl p-5 border border-gray-100 ${stat.color}`}>
            <div className="w-10 h-10 rounded-lg bg-white/60 flex items-center justify-center mb-3">
              <stat.icon size={20} />
            </div>
            <p className="text-2xl font-bold text-gray-900">{stat.value}</p>
            <p className="text-sm font-medium">{stat.label}</p>
            <p className="text-xs text-gray-500 mt-1">{stat.sub}</p>
          </div>
        ))}

        <div className="rounded-2xl p-5 border border-gray-100 bg-white flex items-center gap-4">
          <div className="relative w-16 h-16 shrink-0">
            <svg viewBox="0 0 36 36" className="w-16 h-16 -rotate-90">
              <path className="text-gray-100" stroke="currentColor" strokeWidth="3" fill="none"
                d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" />
              <path className="text-indigo-600" stroke="currentColor" strokeWidth="3" fill="none"
                strokeDasharray={`${Math.min(timesheet.completionPercentage, 100)}, 100`}
                strokeLinecap="round"
                d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" />
            </svg>
            <div className="absolute inset-0 flex items-center justify-center text-xs font-bold text-indigo-700">
              {timesheet.completionPercentage}%
            </div>
          </div>
          <div>
            <p className="text-xs text-gray-500">Total Hours</p>
            <p className="text-lg font-bold text-gray-900">{formatHoursMinutes(timesheet.totalHours)}</p>
            <p className="text-xs text-gray-400">of {Math.round(timesheet.expectedHours)}h</p>
          </div>
        </div>
      </div>

      <div className="bg-white rounded-2xl shadow-sm border border-gray-200 p-6">
        <div className="flex items-center justify-between mb-4">
          <div>
            <h3 className="text-xl font-bold text-gray-900">Timesheet</h3>
            <p className="text-sm text-gray-500">{formatDate(MONTH_START)} - {formatDate(MONTH_END)}</p>
          </div>
          <div className="flex items-center gap-3">
            <div className="flex bg-gray-100 rounded-lg p-1">
              <button
                onClick={() => setView('week')}
                className={`px-3 py-1.5 text-sm font-medium rounded-md ${view === 'week' ? 'bg-indigo-600 text-white' : 'text-gray-600'}`}
              >
                Week View
              </button>
              <button
                onClick={() => setView('month')}
                className={`px-3 py-1.5 text-sm font-medium rounded-md ${view === 'month' ? 'bg-indigo-600 text-white' : 'text-gray-600'}`}
              >
                Month View
              </button>
            </div>
            {view === 'week' && (
              <div className="flex items-center gap-2 border border-gray-200 rounded-lg px-2 py-1.5 text-sm">
                <button onClick={() => setWeekIndex((i) => Math.max(0, i - 1))} className="p-1 hover:bg-gray-100 rounded">
                  <ChevronLeft size={16} />
                </button>
                <span className="font-medium text-gray-700">
                  Week {weekIndex + 1} ({currentWeek[0] ? new Date(currentWeek[0].date).toLocaleDateString('en-GB', { day: '2-digit', month: 'short' }) : ''} - {currentWeek[currentWeek.length - 1] ? new Date(currentWeek[currentWeek.length - 1].date).toLocaleDateString('en-GB', { day: '2-digit', month: 'short' }) : ''})
                </span>
                <button onClick={() => setWeekIndex((i) => Math.min(weeks.length - 1, i + 1))} className="p-1 hover:bg-gray-100 rounded">
                  <ChevronRight size={16} />
                </button>
              </div>
            )}
          </div>
        </div>

        <table className="w-full text-sm">
          <thead>
            <tr className="text-left text-gray-500 border-b border-gray-200">
              <th className="py-2 font-medium">Date</th>
              <th className="py-2 font-medium">Day</th>
              <th className="py-2 font-medium">Type</th>
              <th className="py-2 font-medium">In Time</th>
              <th className="py-2 font-medium">Out Time</th>
              <th className="py-2 font-medium">Hours</th>
              <th className="py-2 font-medium">Status</th>
              <th className="py-2 font-medium">Remarks</th>
            </tr>
          </thead>
          <tbody>
            {(view === 'week' ? currentWeek : timesheet.entries).map((entry: any, idx: number) => {
              const Icon = WORK_TYPE_ICONS[entry.workType] ?? Clock
              const date = new Date(entry.date)
              return (
                <tr key={idx} className="border-b border-gray-100 hover:bg-gray-50">
                  <td className="py-3">{date.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })}</td>
                  <td className="py-3">{date.toLocaleDateString('en-US', { weekday: 'short' })}</td>
                  <td className="py-3">
                    <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-medium ${WORK_TYPE_STYLES[entry.workType] ?? ''}`}>
                      <Icon size={14} /> {entry.workType}
                    </span>
                  </td>
                  <td className="py-3">{formatTime(entry.inTime)}</td>
                  <td className="py-3">{formatTime(entry.outTime)}</td>
                  <td className="py-3">{formatHoursMinutes(entry.hours)}</td>
                  <td className="py-3">
                    <span className="px-2.5 py-1 rounded-full text-xs font-medium bg-emerald-50 text-emerald-700">
                      {entry.status === 'AutoFilled' ? 'Auto-filled' : entry.status}
                    </span>
                  </td>
                  <td className="py-3 text-gray-500">{entry.notes || '–'}</td>
                </tr>
              )
            })}
          </tbody>
          {view === 'week' && (
            <tfoot>
              <tr>
                <td colSpan={5} className="py-3 font-semibold text-gray-700">Total for Week</td>
                <td className="py-3 font-semibold text-gray-900">{formatHoursMinutes(weekTotal)}</td>
                <td colSpan={2} />
              </tr>
            </tfoot>
          )}
        </table>

        <div className="flex items-center justify-between mt-6">
          <button className="flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50">
            <Plus size={16} /> Add Entry
          </button>
          <div className="flex items-center gap-3">
            <button
              onClick={handleRecalculate}
              className="flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50"
            >
              <RotateCw size={16} /> Recalculate
            </button>
            <button
              onClick={handleSubmit}
              className="flex items-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg text-sm font-medium hover:bg-indigo-700"
            >
              <Send size={16} /> Submit Timesheet
            </button>
          </div>
        </div>
      </div>

      <div className="relative overflow-hidden rounded-2xl bg-gradient-to-r from-indigo-50 to-blue-50 border border-indigo-100 p-6 mt-6 flex items-center justify-between gap-6">
        <div className="flex items-center gap-4">
          <svg viewBox="0 0 80 80" className="w-16 h-16 shrink-0">
            <circle cx="40" cy="40" r="38" fill="#EEF2FF" />
            <rect x="18" y="46" width="26" height="18" rx="2" fill="#4338CA" />
            <circle cx="31" cy="34" r="10" fill="#F59E0B" />
            <rect x="42" y="50" width="14" height="10" rx="1" fill="#312E81" />
            <path d="M14 66 Q20 54 28 64" stroke="#22C55E" strokeWidth="3" fill="none" />
          </svg>
          <div>
            <p className="font-bold text-gray-900">Your time tells your story.</p>
            <p className="text-sm text-gray-500">Accurate timesheets help build a better tomorrow.</p>
          </div>
        </div>
        <p className="hidden md:block font-serif italic text-gray-500 text-sm shrink-0">
          "It's not just hours, <span className="text-gray-700">it's progress.</span>"
        </p>
      </div>
    </div>
  )
}
