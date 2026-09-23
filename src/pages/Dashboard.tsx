import { useEffect, useState } from 'react'
import { Upload, AlertCircle, CheckCircle, Download, RefreshCw } from 'lucide-react'
import { API_ENDPOINTS } from '../config'
import { apiClient } from '../auth/apiClient'
import { useAuth } from '../auth/AuthContext'

const SAMPLE_FILES: Record<string, string> = {
  employees: '/samples/employees_sample.csv',
  swipes: '/samples/swipes_sample.csv',
  leaves: '/samples/leaves_sample.csv',
  holidays: '/samples/holidays_sample.csv'
}

// Maps the plural import type used in this page to the singular key the Settings API uses.
const SETTINGS_KEY: Record<string, string> = {
  employees: 'employee',
  swipes: 'swipe',
  leaves: 'leave',
  holidays: 'holiday'
}

const IMPORT_ENDPOINT: Record<string, string> = {
  employees: API_ENDPOINTS.IMPORT_EMPLOYEES,
  swipes: API_ENDPOINTS.IMPORT_SWIPES,
  leaves: API_ENDPOINTS.IMPORT_LEAVES,
  holidays: API_ENDPOINTS.IMPORT_HOLIDAYS
}

const SYNC_ENDPOINT: Record<string, string> = {
  employees: API_ENDPOINTS.SYNC_EMPLOYEES,
  swipes: API_ENDPOINTS.SYNC_SWIPES,
  leaves: API_ENDPOINTS.SYNC_LEAVES,
  holidays: API_ENDPOINTS.SYNC_HOLIDAYS
}

// Holidays are organization-wide, so they're the only type not scoped to the current employee.
const NEEDS_EMPLOYEE_ID: Record<string, boolean> = {
  employees: true,
  swipes: true,
  leaves: true,
  holidays: false
}

export default function Dashboard() {
  const { employee } = useAuth()
  const employeeId = employee!.employeeId
  const [uploadStatus, setUploadStatus] = useState('')
  const [files, setFiles] = useState<Record<string, File | null>>({
    employees: null,
    swipes: null,
    leaves: null,
    holidays: null
  })
  const [settings, setSettings] = useState<Record<string, { sourceType: 'Csv' | 'Api' }> | null>(null)
  const [syncing, setSyncing] = useState<string | null>(null)

  useEffect(() => {
    apiClient.get(API_ENDPOINTS.SETTINGS)
      .then(res => setSettings(res.data))
      .catch(() => setSettings(null))
  }, [])

  const handleFileSelect = (type: string, file: File | null) => {
    setFiles(prev => ({ ...prev, [type]: file }))
  }

  const handleUpload = async (type: string) => {
    const file = files[type]
    if (!file) {
      setUploadStatus(`No file selected for ${type}`)
      return
    }

    const formData = new FormData()
    formData.append('file', file)

    try {
      const params = NEEDS_EMPLOYEE_ID[type] ? { employeeId } : {}
      const response = await apiClient.post(IMPORT_ENDPOINT[type], formData, { params })
      setUploadStatus(`✅ ${response.data.message}`)
      setFiles(prev => ({ ...prev, [type]: null }))
    } catch (error: any) {
      setUploadStatus(`❌ Error uploading ${type}: ${error.response?.data?.message ?? error.message}`)
    }
  }

  const handleSync = async (type: string) => {
    setSyncing(type)
    try {
      const params = NEEDS_EMPLOYEE_ID[type] ? { employeeId } : {}
      const response = await apiClient.post(SYNC_ENDPOINT[type], null, { params })
      setUploadStatus(`✅ ${response.data.message}`)
    } catch (error: any) {
      setUploadStatus(`❌ Error syncing ${type}: ${error.response?.data?.message ?? error.message}`)
    }
    setSyncing(null)
  }

  return (
    <div className="max-w-6xl mx-auto">
      <h2 className="text-3xl font-bold mb-8 text-gray-800">Import Data</h2>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
        {['employees', 'swipes', 'leaves', 'holidays'].map(type => {
          const isApiMode = settings?.[SETTINGS_KEY[type]]?.sourceType === 'Api'

          return (
            <div key={type} className="bg-white rounded-2xl shadow-sm p-6 border border-gray-200 hover:shadow-md transition-shadow">
              <div className="flex items-center gap-4 mb-4">
                <div className="w-12 h-12 bg-indigo-100 rounded-lg flex items-center justify-center">
                  <Upload className="text-indigo-600" size={24} />
                </div>
                <div>
                  <h3 className="text-xl font-semibold capitalize text-gray-800">{type} CSV</h3>
                  {NEEDS_EMPLOYEE_ID[type] && (
                    <p className="text-xs text-gray-400">Scoped to {employeeId} only</p>
                  )}
                </div>
              </div>

              {isApiMode ? (
                <div>
                  <p className="text-sm text-gray-500 mb-4">
                    This data source is configured to sync from a third-party API. Change this in Settings.
                  </p>
                  <button
                    onClick={() => handleSync(type)}
                    disabled={syncing === type}
                    className="w-full flex items-center justify-center gap-2 bg-indigo-600 text-white py-2 rounded-lg hover:bg-indigo-700 font-semibold disabled:opacity-50"
                  >
                    <RefreshCw size={18} className={syncing === type ? 'animate-spin' : ''} />
                    {syncing === type ? 'Syncing...' : `Sync ${type} from API`}
                  </button>
                </div>
              ) : (
                <>
                  <a
                    href={SAMPLE_FILES[type]}
                    download
                    className="inline-flex items-center gap-2 text-sm font-medium text-indigo-600 hover:text-indigo-800 mb-4"
                  >
                    <Download size={16} /> Download sample {type} CSV
                  </a>

                  <input
                    type="file"
                    accept=".csv"
                    onChange={(e) => handleFileSelect(type, e.target.files?.[0] || null)}
                    className="w-full mb-4 p-3 border border-gray-300 rounded-lg"
                  />

                  <button
                    onClick={() => handleUpload(type)}
                    className="w-full bg-indigo-600 text-white py-2 rounded-lg hover:bg-indigo-700 font-semibold"
                  >
                    Upload {type.charAt(0).toUpperCase() + type.slice(1)}
                  </button>
                </>
              )}
            </div>
          )
        })}
      </div>

      {uploadStatus && (
        <div className={`p-4 rounded-lg mb-6 flex items-center gap-3 ${uploadStatus.includes('✅') ? 'bg-green-50 text-green-800 border border-green-200' : 'bg-red-50 text-red-800 border border-red-200'}`}>
          {uploadStatus.includes('✅') ? <CheckCircle size={20} /> : <AlertCircle size={20} />}
          {uploadStatus}
        </div>
      )}
    </div>
  )
}
