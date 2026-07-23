import { useTranslation } from 'react-i18next'

import { DataTableCard } from '../components/data-list/DataTableCard'
import { PageHeader } from '../components/layout/PageHeader'

function VoucherRedemptionsPage() {
  const { t } = useTranslation()

  return (
    <>
      <PageHeader
        eyebrow={t('voucherRedemptions.eyebrow')}
        title={t('voucherRedemptions.title')}
        description={t('voucherRedemptions.description')}
      />
      <DataTableCard className="mt-5 p-8 text-center text-[13px] text-muted-foreground">
        {t('voucherRedemptions.comingSoon')}
      </DataTableCard>
    </>
  )
}

export { VoucherRedemptionsPage }
