export const API_BASE_URL = 'http://localhost:5000/api'

export const API_ENDPOINTS = {
  LOGIN: '/auth/login',
  SSO: '/auth/sso',
  REFRESH: '/auth/refresh',
  LOGOUT: '/auth/logout',
  ME: '/auth/me',
  IMPORT_EMPLOYEES: '/timesheet/import/employees',
  IMPORT_SWIPES: '/timesheet/import/swipes',
  IMPORT_LEAVES: '/timesheet/import/leaves',
  IMPORT_HOLIDAYS: '/timesheet/import/holidays',
  SYNC_EMPLOYEES: '/timesheet/sync/employees',
  SYNC_SWIPES: '/timesheet/sync/swipes',
  SYNC_LEAVES: '/timesheet/sync/leaves',
  SYNC_HOLIDAYS: '/timesheet/sync/holidays',
  GENERATE: '/timesheet/generate',
  RECALCULATE: '/timesheet/recalculate',
  SUBMIT: '/timesheet/submit',
  EMPLOYEES: '/timesheet/employees',
  HEALTH: '/timesheet/health',
  SETTINGS: '/settings'
}
