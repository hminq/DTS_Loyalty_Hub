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
    </>
  )
}

export { SupportPage }
