// Google OAuth Configuration
// This file handles Google authentication integration

const GOOGLE_CLIENT_ID = import.meta.env.VITE_GOOGLE_CLIENT_ID || 'YOUR_GOOGLE_CLIENT_ID_HERE'

/**
 * Initialize Google OAuth
 * Call this when the page loads
 */
export function initializeGoogleAuth() {
  if (!GOOGLE_CLIENT_ID) {
    console.warn('Google Client ID not configured')
    return
  }

  // Load Google Sign-In library
  const script = document.createElement('script')
  script.src = 'https://accounts.google.com/gsi/client'
  script.async = true
  script.defer = true
  document.head.appendChild(script)
}

/**
 * Handle Google Sign-In callback
 * This is called after user authenticates with Google
 */
export async function handleGoogleSignIn(response: any) {
  if (!response.credential) {
    console.error('No credential received from Google')
    return null
  }

  // response.credential is the JWT token from Google
  return response.credential
}

/**
 * Decode Google JWT token (for debugging - use only on frontend, never send to Google)
 */
export function decodeGoogleToken(token: string) {
  try {
    const parts = token.split('.')
    const decoded = JSON.parse(atob(parts[1]))
    return decoded
  } catch (error) {
    console.error('Error decoding token:', error)
    return null
  }
}

/**
 * Store Google token in localStorage
 */
export function storeGoogleToken(token: string) {
  localStorage.setItem('google_id_token', token)
}

/**
 * Get Google token from localStorage
 */
export function getGoogleToken() {
  return localStorage.getItem('google_id_token')
}

/**
 * Clear Google token
 */
export function clearGoogleToken() {
  localStorage.removeItem('google_id_token')
}

export default {
  initializeGoogleAuth,
  handleGoogleSignIn,
  decodeGoogleToken,
  storeGoogleToken,
  getGoogleToken,
  clearGoogleToken
}
