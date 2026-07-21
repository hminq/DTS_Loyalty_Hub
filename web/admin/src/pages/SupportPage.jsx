import { useTranslation } from 'react-i18next'

import { PageHeader } from '../components/layout/PageHeader'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card'

function SupportPage() {
  const { t } = useTranslation()

  return (
    <>
      <PageHeader
        eyebrow={t('support.eyebrow')}
        title={t('support.title')}
        description={t('support.description')}
      />

      <div className="mt-6 grid gap-4 xl:grid-cols-2">
        <Card className="rounded-xl border-border/80 shadow-none">
          <CardHeader className="p-4">
            <CardTitle className="text-sm">{t('support.contactsTitle')}</CardTitle>
            <CardDescription className="text-xs">{t('support.contactsDescription')}</CardDescription>
          </CardHeader>
          <CardContent className="grid gap-3 p-4 pt-0 text-[13px]">
            <div className="rounded-xl border border-border p-3">
              <p className="font-semibold">{t('support.technical')}</p>
              <p className="mt-1 text-muted-foreground">support@loyaltyhub.local</p>
            </div>
            <div className="rounded-xl border border-border p-3">
              <p className="font-semibold">{t('support.operational')}</p>
              <p className="mt-1 text-muted-foreground">ops@loyaltyhub.local</p>
            </div>
          </CardContent>
        </Card>
      </div>
    </>
  )
}

export { SupportPage }
