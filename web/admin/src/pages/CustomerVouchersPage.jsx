import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router-dom'

import { getCustomerVouchers } from '../api/customerAccountsApi'
import { ListPagination } from '../components/data-list/ListPagination'
import { CustomerDataPageHeader } from '../components/customer-accounts/CustomerDataPageHeader'
import { CustomerVouchersTable } from '../components/customer-accounts/CustomerVouchersTable'
import { DataTableCard } from '../components/data-list/DataTableCard'
import { useCustomerPagedResource } from '../hooks/useCustomerPagedResource'
import { Button } from '../components/ui/button'

function CustomerVouchersPage() {
  const { customerId } = useParams()
  const { i18n, t } = useTranslation()

  const {
    page,
    pageSize,
    data,
    meta,
    isLoading,
    isRefreshing,
    errorMessage,
    handlePageChange,
    handlePageSizeChange,
    retry,
  } = useCustomerPagedResource(
    customerId,
    getCustomerVouchers,
    t('customerAccounts.vouchers.error'),
  )

  return (
    <CustomerDataPageHeader customerId={customerId} sectionLabel={t('customerAccounts.sections.vouchers')} t={t}>
      <DataTableCard className="mt-5">
        {errorMessage ? (
          <div className="flex flex-wrap items-center justify-between gap-3 p-4 text-[13px] font-medium text-destructive">
            <p>{errorMessage}</p>
            <Button variant="outline" size="sm" onClick={retry}>
              {t('customerAccounts.retry')}
            </Button>
          </div>
        ) : !isLoading && data.length === 0 ? (
          <div className="p-8 text-center text-[13px] text-muted-foreground">
            {t('customerAccounts.vouchers.empty')}
          </div>
        ) : (
          <>
            <CustomerVouchersTable
              vouchers={data}
              isLoading={isLoading}
              isRefreshing={isRefreshing}
              language={i18n.resolvedLanguage}
              t={t}
            />
            <ListPagination
              meta={meta}
              page={page}
              pageSize={pageSize}
              onPageChange={handlePageChange}
              onPageSizeChange={handlePageSizeChange}
            />
          </>
        )}
      </DataTableCard>
    </CustomerDataPageHeader>
  )
}

export { CustomerVouchersPage }
