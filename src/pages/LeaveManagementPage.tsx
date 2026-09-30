import { useState, useEffect } from 'react'
import { Calendar, Clock, CheckCircle, AlertCircle, Plus, Trash2 } from 'lucide-react'
import { API_ENDPOINTS } from '../config'
import { apiClient } from '../auth/apiClient'
import { useAuth } from '../auth/AuthContext'
import { useToast } from '../components/Toast'
import Loader from '../components/Loader'

export default function LeaveManagementPage() {
  const { employee } = useAuth()
  const { toast, show } = useToast()
  const [loading, setLoading] = useState(false)
  const [leaveRequests, setLeaveRequests] = useState<any[]>([])
  const [balances, setBalances] = useState<any>({})
  const [showForm, setShowForm] = useState(false)
  const [formData, setFormData] = useState({
    leaveType: 'Casual',
    startDate: '',
    endDate: '',
    reason: ''
  })

  useEffect(() => {
    loadData()
  }, [])

  const loadData = async () => {
    setLoading(true)
    try {
      const [requestsRes, balancesRes] = await Promise.all([
        apiClient.get('/api/leavemanagement/requests'),
        apiClient.get('/api/leavemanagement/balances')
      ])
      setLeaveRequests(requestsRes.data)
      setBalances(balancesRes.data)
    } catch (error: any) {
      show(error.message, 'error')
    }
    setLoading(false)
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true)
    try {
      await apiClient.post('/api/leavemanagement/request', {
        ...formData,
        startDate: new Date(formData.startDate),
        endDate: new Date(formData.endDate)
      })
      show('Leave request submitted successfully', 'success')
      setShowForm(false)
      setFormData({ leaveType: 'Casual', startDate: '', endDate: '', reason: '' })
      await loadData()
    } catch (error: any) {
      show(error.message, 'error')
    }
    setLoading(false)
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Approved': return 'bg-green-100 text-green-800'
      case 'Pending': return 'bg-yellow-100 text-yellow-800'
      case 'Rejected': return 'bg-red-100 text-red-800'
      default: return 'bg-gray-100 text-gray-800'
    }
  }

  if (loading && !leaveRequests.length) return <Loader />

  return (
    <div className="max-w-6xl mx-auto px-4 md:px-0">
      <h2 className="text-2xl md:text-3xl font-bold mb-8 text-gray-800">Leave Management</h2>

      {/* Leave Balances */}
      <div className="grid grid-cols-2 md:grid-cols-3 gap-3 md:gap-4 mb-8">
        {Object.entries(balances).map(([type, balance]: any) => (
          <div key={type} className="bg-white rounded-lg p-4 border border-gray-200">
            <p className="text-xs md:text-sm text-gray-600 mb-2">{type}</p>
            <p className="text-lg md:text-2xl font-bold text-indigo-600">{balance.daysRemaining || 0}</p>
            <p className="text-xs text-gray-500">of {balance.totalDaysAllowed || 0} days</p>
          </div>
        ))}
      </div>

      {/* New Request Form */}
      {!showForm ? (
        <button
          onClick={() => setShowForm(true)}
          className="flex items-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 mb-6 w-full md:w-auto"
        >
          <Plus size={20} /> Request Leave
        </button>
      ) : (
        <form onSubmit={handleSubmit} className="bg-white rounded-lg p-4 md:p-6 border border-gray-200 mb-6">
          <h3 className="text-lg font-semibold mb-4">Request New Leave</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-semibold mb-2">Leave Type</label>
              <select
                value={formData.leaveType}
                onChange={(e) => setFormData({ ...formData, leaveType: e.target.value })}
                className="w-full p-2 border border-gray-300 rounded-lg"
              >
                <option>Casual</option>
                <option>Sick</option>
                <option>Earned</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-semibold mb-2">Start Date</label>
              <input
                type="date"
                value={formData.startDate}
                onChange={(e) => setFormData({ ...formData, startDate: e.target.value })}
                required
                className="w-full p-2 border border-gray-300 rounded-lg"
              />
            </div>
            <div>
              <label className="block text-sm font-semibold mb-2">End Date</label>
              <input
                type="date"
                value={formData.endDate}
                onChange={(e) => setFormData({ ...formData, endDate: e.target.value })}
                required
                className="w-full p-2 border border-gray-300 rounded-lg"
              />
            </div>
            <div>
              <label className="block text-sm font-semibold mb-2">Reason</label>
              <input
                type="text"
                value={formData.reason}
                onChange={(e) => setFormData({ ...formData, reason: e.target.value })}
                placeholder="Optional"
                className="w-full p-2 border border-gray-300 rounded-lg"
              />
            </div>
          </div>
          <div className="flex gap-3 mt-4">
            <button type="submit" className="flex-1 bg-indigo-600 text-white py-2 rounded-lg hover:bg-indigo-700">
              Submit Request
            </button>
            <button
              type="button"
              onClick={() => setShowForm(false)}
              className="flex-1 bg-gray-200 text-gray-800 py-2 rounded-lg hover:bg-gray-300"
            >
              Cancel
            </button>
          </div>
        </form>
      )}

      {/* Leave Requests Table */}
      <div className="bg-white rounded-lg border border-gray-200 overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 border-b">
              <tr>
                <th className="text-left p-3 font-semibold">Type</th>
                <th className="text-left p-3 font-semibold hidden md:table-cell">Dates</th>
                <th className="text-left p-3 font-semibold">Days</th>
                <th className="text-left p-3 font-semibold">Status</th>
                <th className="text-left p-3 font-semibold">Action</th>
              </tr>
            </thead>
            <tbody>
              {leaveRequests.map((request) => (
                <tr key={request.id} className="border-b hover:bg-gray-50">
                  <td className="p-3">
                    <span className="px-2 py-1 bg-blue-100 text-blue-800 rounded text-xs font-semibold">
                      {request.leaveType}
                    </span>
                  </td>
                  <td className="p-3 hidden md:table-cell">
                    <span className="text-gray-600 text-xs md:text-sm">
                      {new Date(request.startDate).toLocaleDateString()} - {new Date(request.endDate).toLocaleDateString()}
                    </span>
                  </td>
                  <td className="p-3 text-sm">{Math.ceil((new Date(request.endDate).getTime() - new Date(request.startDate).getTime()) / (1000 * 60 * 60 * 24))}</td>
                  <td className="p-3">
                    <span className={`px-2 py-1 rounded text-xs font-semibold ${getStatusColor(request.status)}`}>
                      {request.status}
                    </span>
                  </td>
                  <td className="p-3">
                    {request.status === 'Pending' && (
                      <button className="text-red-600 hover:bg-red-50 p-1 rounded">
                        <Trash2 size={16} />
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {toast && <Toast {...toast} onClose={() => {}} />}
    </div>
  )
}

function Toast(props: any) {
  return <div>{props.message}</div>
}
