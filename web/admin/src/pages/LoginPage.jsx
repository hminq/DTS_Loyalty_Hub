import { useState } from 'react'
import {
  ArrowRightIcon,
  CircleNotchIcon,
  EyeIcon,
  EyeSlashIcon,
  LockKeyIcon,
  UserIcon,
  WarningCircleIcon,
} from '@phosphor-icons/react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'

import { login, toFieldErrorMap } from '../api'
import { BrandMark } from '../components/BrandMark'
import { LanguageSwitcher } from '../components/LanguageSwitcher'
import { Button } from '../components/ui/button'
import { Input } from '../components/ui/input'
import { storageKeys } from '../config/storageKeys'

function LoginPage() {
  const [showPassword, setShowPassword] = useState(false)
  const [values, setValues] = useState({ username: '', password: '' })
  const [fieldErrors, setFieldErrors] = useState({})
  const [formError, setFormError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const { t } = useTranslation()
  const navigate = useNavigate()

  function handleChange(event) {
    const { name, value } = event.target
    setValues((current) => ({ ...current, [name]: value }))
    setFieldErrors((current) => ({ ...current, [name]: undefined }))
    setFormError('')
  }

  async function handleSubmit(event) {
    event.preventDefault()
    setFieldErrors({})
    setFormError('')
    setIsSubmitting(true)

    try {
      const result = await login(values)
      localStorage.setItem(storageKeys.accessToken, result.accessToken)
      navigate('/dashboard', { replace: true })
    } catch (error) {
      const nextFieldErrors = toFieldErrorMap(error.details)
      setFieldErrors(nextFieldErrors)

      if (Object.keys(nextFieldErrors).length === 0) {
        setFormError(error.message || t('login.unexpectedError'))
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="relative grid min-h-screen place-items-center overflow-hidden bg-[#f3f7fb] px-5 py-10">
      <LanguageSwitcher />
      <div className="pointer-events-none absolute -left-40 -top-52 size-[540px] rounded-full bg-[#dfe8f4]" />
      <div className="pointer-events-none absolute -bottom-72 -right-44 size-[680px] rounded-full bg-[#e2eaf5]" />
      <div className="pointer-events-none absolute left-[58%] top-[-18rem] size-[520px] rounded-full bg-white/55" />

      <section className="relative z-10 w-full max-w-md overflow-hidden rounded-2xl border border-white/80 bg-card shadow-[0_24px_70px_rgba(16,42,86,0.14)]">
        <div className="border-b border-border bg-muted/45 px-7 py-7 sm:px-9">
          <BrandMark />
        </div>

        <div className="px-7 py-8 sm:px-9 sm:py-9">
          <div className="text-center">
            <p className="text-xs font-semibold uppercase tracking-[0.18em] text-primary">{t('login.eyebrow')}</p>
            <h1 className="mt-3 text-3xl font-semibold tracking-[-0.03em]">{t('login.title')}</h1>
            <p className="mt-3 text-sm text-muted-foreground">{t('login.description')}</p>
          </div>

          <form className="mt-8 grid gap-5" onSubmit={handleSubmit}>
            {formError && (
              <div className="flex items-start gap-2.5 rounded-lg border border-destructive/20 bg-destructive/5 px-3 py-2.5 text-sm text-destructive" role="alert">
                <WarningCircleIcon className="mt-0.5 shrink-0" size={17} weight="fill" />
                <span className="leading-5">{formError}</span>
              </div>
            )}

            <label className="grid gap-2 text-sm font-medium" htmlFor="username">
              {t('login.username')}
              <div className="relative">
                <UserIcon className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground" size={17} />
                <Input
                  id="username"
                  name="username"
                  value={values.username}
                  onChange={handleChange}
                  autoComplete="username"
                  className="h-11 bg-white pl-10 aria-invalid:border-destructive aria-invalid:ring-destructive/20"
                  placeholder={t('login.usernamePlaceholder')}
                  aria-invalid={Boolean(fieldErrors.username)}
                  aria-describedby={fieldErrors.username ? 'username-error' : undefined}
                />
              </div>
              {fieldErrors.username && <span id="username-error" className="text-xs font-normal text-destructive">{fieldErrors.username}</span>}
            </label>

            <label className="grid gap-2 text-sm font-medium" htmlFor="password">
              {t('login.password')}
              <div className="relative">
                <LockKeyIcon className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground" size={17} />
                <Input
                  id="password"
                  name="password"
                  value={values.password}
                  onChange={handleChange}
                  type={showPassword ? 'text' : 'password'}
                  autoComplete="current-password"
                  className="h-11 bg-white pl-10 pr-10 aria-invalid:border-destructive aria-invalid:ring-destructive/20"
                  placeholder={t('login.passwordPlaceholder')}
                  aria-invalid={Boolean(fieldErrors.password)}
                  aria-describedby={fieldErrors.password ? 'password-error' : undefined}
                />
                <button
                  type="button"
                  className="absolute right-1.5 top-1/2 grid size-8 -translate-y-1/2 place-items-center rounded-md text-muted-foreground outline-none transition-colors hover:bg-muted hover:text-foreground focus-visible:ring-2 focus-visible:ring-ring"
                  onClick={() => setShowPassword((current) => !current)}
                  aria-label={showPassword ? t('login.hidePassword') : t('login.showPassword')}
                >
                  {showPassword ? <EyeSlashIcon size={17} /> : <EyeIcon size={17} />}
                </button>
              </div>
              {fieldErrors.password && <span id="password-error" className="text-xs font-normal text-destructive">{fieldErrors.password}</span>}
            </label>

            <Button className="mt-2 h-11 w-full" type="submit" disabled={isSubmitting}>
              {isSubmitting ? (
                <>
                  <CircleNotchIcon className="animate-spin" />
                  {t('login.submitting')}
                </>
              ) : (
                <>
                  {t('login.submit')}
                  <ArrowRightIcon weight="bold" />
                </>
              )}
            </Button>
          </form>

          <p className="mt-7 text-center text-xs leading-5 text-muted-foreground">
            {t('login.notice')}
          </p>
        </div>
      </section>
    </main>
  )
}

export { LoginPage }
