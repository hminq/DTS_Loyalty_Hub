import { useTranslation } from 'react-i18next'

import { PageHeader } from '../components/layout/PageHeader'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card'
import { cn } from '../lib/utils'

function SettingsPage() {
  const { i18n, t } = useTranslation()
  const currentLanguage = i18n.resolvedLanguage || 'en'

  return (
    <>
      <PageHeader
        eyebrow={t('settings.eyebrow')}
        title={t('settings.title')}
        description={t('settings.description')}
      />

      <div className="mt-6 grid gap-4 xl:grid-cols-2">
        <Card className="rounded-xl border-border/80 shadow-none">
          <CardHeader className="p-4">
            <CardTitle className="text-sm">{t('settings.languageTitle')}</CardTitle>
            <CardDescription className="text-xs">
              {t('settings.languageDescription')}
            </CardDescription>
          </CardHeader>
          <CardContent className="p-4 pt-0">
            <div className="inline-flex rounded-lg border border-border bg-background p-1">
              {['en', 'vi'].map((language) => (
                <button
                  key={language}
                  type="button"
                  className={cn(
                    'grid h-8 min-w-12 place-items-center rounded-md px-3 text-xs font-semibold uppercase transition-colors',
                    currentLanguage === language
                      ? 'bg-primary text-primary-foreground shadow-xs'
                      : 'text-muted-foreground hover:bg-muted hover:text-foreground',
                  )}
                  onClick={() => i18n.changeLanguage(language)}
                  aria-pressed={currentLanguage === language}
                >
                  {language}
                </button>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>
    </>
  )
}

export { SettingsPage }
