import {
  ArrowLeftIcon,
  CompassIcon,
  SignOutIcon,
} from '@phosphor-icons/react'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'

import { BrandMark } from '../components/BrandMark'
import { LanguageSwitcher } from '../components/LanguageSwitcher'
import { Button } from '../components/ui/button'
import { logoutSession } from '../lib/logout'

function NotFoundPage() {
  const navigate = useNavigate()
  const { t } = useTranslation()
  const [logoutError, setLogoutError] = useState('')
  const [isLoggingOut, setIsLoggingOut] = useState(false)

  function handleReturn() {
    if (window.history.length > 1) {
      navigate(-1)
      return
    }

    navigate('/login', { replace: true })
  }

  async function handleLogout() {
    setIsLoggingOut(true)
    setLogoutError('')

    const result = await logoutSession()

    setIsLoggingOut(false)

    if (result.ok) {
      navigate('/login', { replace: true })
      return
    }

    setLogoutError(t('notFound.logoutError'))
  }

  return (
    <main className="relative grid min-h-screen place-items-center overflow-hidden bg-background px-6 py-12">
      <LanguageSwitcher />
      <div className="absolute inset-x-0 top-0 h-1 bg-primary" />
      <div className="absolute left-1/2 top-1/2 -z-0 size-[480px] -translate-x-1/2 -translate-y-1/2 rounded-full bg-accent/55 blur-3xl" />

      <div className="relative z-10 w-full max-w-xl text-center">
        <BrandMark className="mb-16 justify-center" />

        <div className="mx-auto grid size-14 place-items-center rounded-2xl border border-border bg-card shadow-sm">
          <CompassIcon size={28} weight="duotone" className="text-primary" />
        </div>

        <p className="mt-8 text-xs font-semibold uppercase tracking-[0.22em] text-primary">{t('notFound.eyebrow')}</p>
        <h1 className="mt-3 text-4xl font-semibold tracking-[-0.035em] sm:text-5xl">{t('notFound.title')}</h1>
        <p className="mx-auto mt-5 max-w-md text-sm leading-6 text-muted-foreground">
          {t('notFound.description')}
        </p>

        <div className="mt-9 flex flex-col justify-center gap-3 sm:flex-row">
          <Button className="sm:min-w-36" onClick={handleReturn}>
            <ArrowLeftIcon size={17} weight="bold" />
            {t('notFound.goBack')}
          </Button>
          <Button className="sm:min-w-36" variant="outline" onClick={handleLogout} disabled={isLoggingOut}>
            <SignOutIcon size={17} />
            {t('notFound.logout')}
          </Button>
        </div>
        {logoutError ? (
          <p className="mt-5 text-sm font-medium text-destructive">{logoutError}</p>
        ) : null}

        <p className="mt-14 text-xs text-muted-foreground">{t('notFound.footer')}</p>
      </div>
    </main>
  )
}

export { NotFoundPage }
