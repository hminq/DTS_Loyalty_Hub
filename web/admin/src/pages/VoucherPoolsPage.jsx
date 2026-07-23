import { useTranslation } from 'react-i18next'

import { DataTableCard } from '../components/data-list/DataTableCard'
import { PageHeader } from '../components/layout/PageHeader'

function VoucherPoolsPage() {
  const { t } = useTranslation()

  return (
    <>
      <PageHeader
        eyebrow={t('voucherPools.eyebrow')}
        title={t('voucherPools.title')}
        description={t('voucherPools.description')}
      />
      <DataTableCard className="mt-5 p-8 text-center text-[13px] text-muted-foreground">
        {t('voucherPools.comingSoon')}
      </DataTableCard>
    </>
  )
}

export { VoucherPoolsPage }
