import { SignOutIcon, SquaresFourIcon } from '@phosphor-icons/react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'

import { BrandMark } from '../components/BrandMark'
import { LanguageSwitcher } from '../components/LanguageSwitcher'
import { Button } from '../components/ui/button'
import { logoutAdmin } from '../lib/logout'

function DashboardPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const [logoutError, setLogoutError] = useState('')
  const [isLoggingOut, setIsLoggingOut] = useState(false)

  async function handleLogout() {
    setIsLoggingOut(true)
    setLogoutError('')

    const result = await logoutAdmin()

    setIsLoggingOut(false)

    if (result.ok) {
      navigate('/login', { replace: true })
      return
    }

    setLogoutError(t('dashboard.logoutError'))
  }

  return (
    <main className="min-h-screen bg-muted/40">
      <LanguageSwitcher />
      <header className="border-b border-border bg-background">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-4 lg:px-8">
          <BrandMark />
          <Button className="mr-28" variant="outline" size="sm" onClick={handleLogout} disabled={isLoggingOut}>
            <SignOutIcon />
            {t('dashboard.logout')}
          </Button>
        </div>
      </header>

      <section className="mx-auto grid max-w-6xl place-items-center px-6 py-28 text-center lg:px-8">
        <div className="grid size-14 place-items-center rounded-2xl bg-accent text-primary">
          <SquaresFourIcon size={28} weight="duotone" />
        </div>
        <p className="mt-7 text-xs font-semibold uppercase tracking-[0.18em] text-primary">{t('dashboard.eyebrow')}</p>
        <h1 className="mt-3 text-3xl font-semibold tracking-[-0.03em]">{t('dashboard.title')}</h1>
        <p className="mt-4 max-w-md text-sm leading-6 text-muted-foreground">{t('dashboard.description')}</p>
        {logoutError ? (
          <p className="mt-6 text-sm font-medium text-destructive">{logoutError}</p>
        ) : null}
      </section>
    </main>
  )
}

export { DashboardPage }
