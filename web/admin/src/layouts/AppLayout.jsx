import { CircleNotchIcon } from '@phosphor-icons/react'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Outlet, useNavigate } from 'react-router-dom'

import { getCurrentAdmin } from '../api/authApi'
import { useAuth } from '../auth/AuthContext'
import { authEvents } from '../auth/authEvents'
import { Sidebar } from '../components/layout/Sidebar'
import { Button } from '../components/ui/button'
import { getVisibleNavigation, navigationItems } from '../constants/navigation'
import { logoutSession } from '../lib/logout'

function AppLayout() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { clearAccessToken } = useAuth()
  const [currentAdmin, setCurrentAdmin] = useState(null)
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState(null)
  const [logoutError, setLogoutError] = useState('')
  const [isLoggingOut, setIsLoggingOut] = useState(false)
  const permissionCodes = currentAdmin?.permissions ?? []
  const visibleNavigationItems = useMemo(
    () => getVisibleNavigation(navigationItems, permissionCodes),
    [permissionCodes],
  )

  const refreshCurrentAdmin = useCallback(async () => {
    try {
      const admin = await getCurrentAdmin()
      setCurrentAdmin(admin)
      setLoadError(null)
      return admin
    } catch (error) {
      if (error.status === 401) {
        clearAccessToken()
        return null
      }

      setLoadError(error)
      throw error
    }
  }, [clearAccessToken])

  useEffect(() => {
    let isMounted = true

    async function loadCurrentAdmin() {
      try {
        await refreshCurrentAdmin()
      } catch {
        // The shared error state is set by refreshCurrentAdmin.
      } finally {
        if (isMounted) {
          setIsLoading(false)
        }
      }
    }

    loadCurrentAdmin()
    return () => { isMounted = false }
  }, [refreshCurrentAdmin])

  useEffect(() => {
    function refreshPermissions() {
      refreshCurrentAdmin().catch(() => {
        // refreshCurrentAdmin owns the visible error state.
      })
    }

    window.addEventListener(authEvents.permissionsStale, refreshPermissions)
    window.addEventListener('focus', refreshPermissions)

    return () => {
      window.removeEventListener(authEvents.permissionsStale, refreshPermissions)
      window.removeEventListener('focus', refreshPermissions)
    }
  }, [refreshCurrentAdmin])

  async function handleLogout() {
    setIsLoggingOut(true)
    setLogoutError('')

    const result = await logoutSession()

    setIsLoggingOut(false)

    if (result.ok) {
      clearAccessToken()
      navigate('/login', { replace: true })
      return
    }

    setLogoutError(t('dashboard.logoutError'))
  }

  function hasPermission(permissionCode) {
    return permissionCodes.includes(permissionCode)
  }

  if (isLoading) {
    return (
      <main className="grid min-h-screen place-items-center bg-background text-foreground">
        <div className="flex items-center gap-2 text-sm font-medium text-muted-foreground">
          <CircleNotchIcon className="animate-spin" size={18} />
          {t('common.loadingWorkspace')}
        </div>
      </main>
    )
  }

  return (
    <main className="min-h-screen bg-background text-foreground">
      <div className="flex min-h-screen">
        <Sidebar
          admin={currentAdmin}
          navigationItems={visibleNavigationItems}
          isLoggingOut={isLoggingOut}
          logoutLabel={t('dashboard.logout')}
          onLogout={handleLogout}
        />

        <section className="min-w-0 flex-1 bg-background px-5 py-6 lg:px-9">
          <div className="mb-4 flex justify-end lg:hidden">
            <Button variant="outline" size="sm" onClick={handleLogout} disabled={isLoggingOut}>
              {t('dashboard.logout')}
            </Button>
          </div>

          {logoutError ? (
            <p className="mb-5 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-sm font-medium text-destructive">
              {logoutError}
            </p>
          ) : null}
          {loadError ? (
            <p className="mb-5 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-sm font-medium text-destructive">
              {loadError.message || t('errors.loadCurrentAccount')}
            </p>
          ) : null}

          <Outlet context={{ currentAdmin, hasPermission, refreshCurrentAdmin }} />
        </section>
      </div>
    </main>
  )
}

export { AppLayout }
