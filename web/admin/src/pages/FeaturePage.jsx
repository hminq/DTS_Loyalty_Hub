import { useTranslation } from 'react-i18next'

import { PageHeader } from '../components/layout/PageHeader'
import { Card, CardContent } from '../components/ui/card'

function FeaturePage({ eyebrowKey, titleKey, descriptionKey }) {
  const { t } = useTranslation()

  return (
    <>
      <PageHeader
        eyebrow={t(eyebrowKey)}
        title={t(titleKey)}
        description={t(descriptionKey)}
      />
      <Card className="mt-6 rounded-xl border-border/80 shadow-none">
        <CardContent className="p-5 text-[13px] text-muted-foreground">
          {t('common.featureNotImplemented')}
        </CardContent>
      </Card>
    </>
  )
}

export { FeaturePage }
