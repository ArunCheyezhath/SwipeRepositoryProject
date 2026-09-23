import { useEffect, useState } from 'react'
import { Save, CheckCircle, AlertCircle } from 'lucide-react'
import { API_ENDPOINTS } from '../config'
import { apiClient } from '../auth/apiClient'

type SourceType = 'Csv' | 'Api'

interface DataSourceSettings {
  sourceType: SourceType
  apiUrl: string
  apiToken: string
}

const DATA_TYPES: { key: string; label: string; description: string }[] = [
  { key: 'employee', label: 'Employee Data', description: 'Employee master data (ID, name, email, standard hours)' },
  { key: 'swipe', label: 'Swipe Data', description: 'Daily in/out swipe records used to calculate office hours' },
  { key: 'leave', label: 'Leave Data', description: 'Approved/pending leave records' },
  { key: 'holiday', label: 'Holiday Data', description: 'Organization-wide holiday calendar' }
]

export default function SettingsPage() {
  const [settings, setSettings] = useState<Record<string, DataSourceSettings> | null>(null)
  const [saving, setSaving] = useState<string | null>(null)
  const [message, setMessage] = useState('')

  useEffect(() => {
    apiClient.get(API_ENDPOINTS.SETTINGS).then(res => setSettings(res.data))
  }, [])

  const updateLocal = (key: string, patch: Partial<DataSourceSettings>) => {
    setSettings(prev => prev && { ...prev, [key]: { ...prev[key], ...patch } })
  }

  const handleSave = async (key: string) => {
    if (!settings) return
    setSaving(key)
    try {
      await apiClient.put(`${API_ENDPOINTS.SETTINGS}/${key}`, settings[key])
      setMessage(`✅ ${key} settings saved`)
    } catch (error: any) {
      setMessage(`❌ Error saving ${key} settings: ${error.message}`)
    }
    setSaving(null)
  }

  if (!settings) {
    return <div className="max-w-4xl mx-auto py-20 text-center text-gray-500">Loading settings…</div>
  }

  return (
    <div className="max-w-4xl mx-auto">
      <h2 className="text-3xl font-bold mb-2 text-gray-800">Settings</h2>
      <p className="text-gray-500 mb-8">Choose whether each data type is imported via CSV upload or synced from a third-party API.</p>

      <div className="space-y-6">
        {DATA_TYPES.map(({ key, label, description }) => {
          const value = settings[key]
          return (
            <div key={key} className="bg-white rounded-2xl shadow-sm border border-gray-200 p-6">
              <h3 className="text-lg font-semibold text-gray-800">{label}</h3>
              <p className="text-sm text-gray-500 mb-4">{description}</p>

              <div className="flex gap-4 mb-4">
                <label className="flex items-center gap-2 text-sm font-medium text-gray-700">
                  <input
                    type="radio"
                    name={`source-${key}`}
                    checked={value.sourceType === 'Csv'}
                    onChange={() => updateLocal(key, { sourceType: 'Csv' })}
                  />
                  CSV Upload
                </label>
                <label className="flex items-center gap-2 text-sm font-medium text-gray-700">
                  <input
                    type="radio"
                    name={`source-${key}`}
                    checked={value.sourceType === 'Api'}
                    onChange={() => updateLocal(key, { sourceType: 'Api' })}
                  />
                  Third-Party API
                </label>
              </div>

              {value.sourceType === 'Api' && (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
                  <div>
                    <label className="block text-xs font-semibold text-gray-500 mb-1">API URL</label>
                    <input
                      type="text"
                      value={value.apiUrl}
                      onChange={(e) => updateLocal(key, { apiUrl: e.target.value })}
                      placeholder="https://hris.example.com/api/employees"
                      className="w-full p-2.5 border border-gray-300 rounded-lg text-sm"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-semibold text-gray-500 mb-1">API Token</label>
                    <input
                      type="password"
                      value={value.apiToken}
                      onChange={(e) => updateLocal(key, { apiToken: e.target.value })}
                      placeholder="Bearer token"
                      className="w-full p-2.5 border border-gray-300 rounded-lg text-sm"
                    />
                  </div>
                </div>
              )}

              <button
                onClick={() => handleSave(key)}
                disabled={saving === key}
                className="flex items-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg text-sm font-medium hover:bg-indigo-700 disabled:opacity-50"
              >
                <Save size={16} /> {saving === key ? 'Saving...' : 'Save'}
              </button>
            </div>
          )
        })}
      </div>

      {message && (
        <div className={`mt-6 p-4 rounded-lg flex items-center gap-3 ${message.includes('✅') ? 'bg-green-50 text-green-800 border border-green-200' : 'bg-red-50 text-red-800 border border-red-200'}`}>
          {message.includes('✅') ? <CheckCircle size={20} /> : <AlertCircle size={20} />}
          {message}
        </div>
      )}
    </div>
  )
}
