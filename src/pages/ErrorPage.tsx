import { Home, AlertCircle } from 'lucide-react'

export default function ErrorPage({ statusCode = 404, message = 'Page not found' }) {
  return (
    <div className="min-h-screen bg-gradient-to-br from-indigo-50 to-blue-50 flex items-center justify-center px-4">
      <div className="text-center">
        <div className="flex justify-center mb-6">
          <div className="relative">
            <div className="w-24 h-24 bg-red-100 rounded-full flex items-center justify-center">
              <AlertCircle size={48} className="text-red-600" />
            </div>
          </div>
        </div>

        <h1 className="text-6xl font-bold text-gray-900 mb-2">{statusCode}</h1>
        <h2 className="text-2xl font-semibold text-gray-700 mb-4">{message}</h2>
        <p className="text-gray-600 max-w-md mx-auto mb-8">
          We couldn't find what you're looking for. Don't worry, let's get you back on track.
        </p>

        <a
          href="/"
          className="inline-flex items-center gap-2 px-6 py-3 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors font-semibold"
        >
          <Home size={20} />
          Back to Home
        </a>
      </div>
    </div>
  )
}
