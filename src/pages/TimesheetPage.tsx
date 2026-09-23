import { useState } from 'react'
import { Calendar, Download, Send } from 'lucide-react'
import { API_ENDPOINTS } from '../config'
import { apiClient } from '../auth/apiClient'
import { useAuth } from '../auth/AuthContext'

export default function TimesheetPage() {
  const { employee } = useAuth()
  const [employeeId, setEmployeeId] = useState(employee!.employeeId)
  const [startDate, setStartDate] = useState('2025-09-15')
  const [endDate, setEndDate] = useState('2025-09-30')
  const [timesheet, setTimesheet] = useState<any>(null)
  const [loading, setLoading] = useState(false)

  const handleGenerate = async () => {
    setLoading(true)
    try {
      const response = await apiClient.post(
        API_ENDPOINTS.GENERATE,
        null,
        { params: { employeeId, startDate, endDate } }
      )
      setTimesheet(response.data)
    } catch (error: any) {
      alert(`Error: ${error.message}`)
    }
    setLoading(false)
  }

  const handleSubmit = async () => {
    if (!timesheet) return
    try {
      await apiClient.post(API_ENDPOINTS.SUBMIT, timesheet)
      alert('Timesheet submitted successfully!')
    } catch (error: any) {
      alert(`Error: ${error.message}`)
    }
  }

  return (
    <div className="max-w-6xl mx-auto">
      <h2 className="text-3xl font-bold mb-8 text-gray-800">My Timesheet</h2>

      <div className="bg-white rounded-2xl shadow-sm p-6 mb-8 border border-gray-200">
        <div className="grid grid-cols-3 gap-4 mb-6">
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">Employee ID</label>
            <input
              type="text"
              value={employeeId}
              onChange={(e) => setEmployeeId(e.target.value)}
              className="w-full p-3 border border-gray-300 rounded-lg"
            />
          </div>
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">Start Date</label>
            <input type="date" value={startDate} onChange={(e) => setStartDate(e.target.value)} className="w-full p-3 border border-gray-300 rounded-lg" />
          </div>
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">End Date</label>
            <input type="date" value={endDate} onChange={(e) => setEndDate(e.target.value)} className="w-full p-3 border border-gray-300 rounded-lg" />
          </div>
        </div>
        <button
          onClick={handleGenerate}
          disabled={loading}
          className="w-full bg-indigo-600 text-white py-3 rounded-lg hover:bg-indigo-700 font-semibold disabled:opacity-50"
        >
          {loading ? 'Generating...' : 'Generate Timesheet'}
        </button>
      </div>

      {timesheet && (
        <div className="bg-white rounded-2xl shadow-sm p-6 border border-gray-200">
          <div className="grid grid-cols-3 gap-4 mb-6">
            <div className="bg-blue-50 p-4 rounded-lg">
              <p className="text-sm text-gray-600">Total Hours</p>
              <p className="text-2xl font-bold text-blue-600">{timesheet.totalHours?.toFixed(2)}</p>
            </div>
            <div className="bg-green-50 p-4 rounded-lg">
              <p className="text-sm text-gray-600">Office Hours</p>
              <p className="text-2xl font-bold text-green-600">{timesheet.officeHours?.toFixed(2)}</p>
            </div>
            <div className="bg-purple-50 p-4 rounded-lg">
              <p className="text-sm text-gray-600">WFH Hours</p>
              <p className="text-2xl font-bold text-purple-600">{timesheet.wfhHours?.toFixed(2)}</p>
            </div>
          </div>

          <table className="w-full text-sm">
            <thead>
              <tr className="border-b-2 border-gray-200">
                <th className="text-left p-3">Date</th>
                <th className="text-left p-3">Type</th>
                <th className="text-left p-3">Hours</th>
              </tr>
            </thead>
            <tbody>
              {timesheet.entries?.map((entry: any, idx: number) => (
                <tr key={idx} className="border-b border-gray-200 hover:bg-gray-50">
                  <td className="p-3">{new Date(entry.date).toLocaleDateString()}</td>
                  <td className="p-3"><span className="px-3 py-1 bg-indigo-100 text-indigo-800 rounded-full text-xs font-semibold">{entry.workType}</span></td>
                  <td className="p-3">{entry.hours?.toFixed(2)}</td>
                </tr>
              ))}
            </tbody>
          </table>

          <div className="flex gap-4 mt-6">
            <button className="flex-1 bg-gray-100 text-gray-800 py-3 rounded-lg hover:bg-gray-200 font-semibold flex items-center justify-center gap-2">
              <Download size={20} /> Export
            </button>
            <button
              onClick={handleSubmit}
              className="flex-1 bg-green-600 text-white py-3 rounded-lg hover:bg-green-700 font-semibold flex items-center justify-center gap-2"
            >
              <Send size={20} /> Submit
            </button>
          </div>
        </div>
      )}
    </div>
  )
}
