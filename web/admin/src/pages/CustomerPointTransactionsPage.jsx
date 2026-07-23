import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router-dom'

import { getCustomerPointTransactions } from '../api/customerAccountsApi'
import { ListPagination } from '../components/data-list/ListPagination'
import { CustomerDataPageHeader } from '../components/customer-accounts/CustomerDataPageHeader'
import { CustomerPointTransactionsTable } from '../components/customer-accounts/CustomerPointTransactionsTable'
import { DataTableCard } from '../components/data-list/DataTableCard'
import { useCustomerPagedResource } from '../hooks/useCustomerPagedResource'
import { Button } from '../components/ui/button'

function CustomerPointTransactionsPage() {
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
    getCustomerPointTransactions,
    t('customerAccounts.transactions.error'),
  )

  return (
    <CustomerDataPageHeader customerId={customerId} sectionLabel={t('customerAccounts.sections.pointTransactions')} t={t}>
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
            {t('customerAccounts.pointTransactions.empty')}
          </div>
        ) : (
          <>
            <CustomerPointTransactionsTable
              transactions={data}
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

export { CustomerPointTransactionsPage }
