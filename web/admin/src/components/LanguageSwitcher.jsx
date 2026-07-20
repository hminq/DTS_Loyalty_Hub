import { useTranslation } from 'react-i18next'

import { cn } from '../lib/utils'

const languages = [
  { code: 'en', label: 'EN' },
  { code: 'vi', label: 'VI' },
]

function LanguageSwitcher({ className }) {
  const { i18n, t } = useTranslation()
  const currentLanguage = i18n.resolvedLanguage || 'en'

  return (
    <div
      className={cn('fixed right-5 top-5 z-50 inline-flex items-center rounded-lg border border-border bg-background p-1 shadow-sm', className)}
      aria-label={t('common.language')}
    >
      {languages.map(({ code, label }) => (
        <button
          key={code}
          type="button"
          className={cn(
            'grid h-7 min-w-9 place-items-center rounded-md px-2 text-xs font-semibold leading-none outline-none transition-colors focus-visible:ring-2 focus-visible:ring-ring',
            currentLanguage === code
              ? 'bg-primary text-primary-foreground shadow-xs'
              : 'text-muted-foreground hover:bg-muted hover:text-foreground',
          )}
          onClick={() => i18n.changeLanguage(code)}
          aria-pressed={currentLanguage === code}
        >
          {label}
        </button>
      ))}
    </div>
  )
}

export { LanguageSwitcher }
