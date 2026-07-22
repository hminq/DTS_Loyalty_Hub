import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router-dom'

import { getCustomerVoucherRedemptions } from '../api/customerAccountsApi'
import { ListPagination } from '../components/data-list/ListPagination'
import { CustomerDataPageHeader } from '../components/customer-accounts/CustomerDataPageHeader'
import { CustomerVoucherRedemptionsTable } from '../components/customer-accounts/CustomerVoucherRedemptionsTable'
import { useCustomerPagedResource } from '../hooks/useCustomerPagedResource'
import { Button } from '../components/ui/button'
import { Card, CardContent } from '../components/ui/card'

function CustomerVoucherRedemptionsPage() {
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
    getCustomerVoucherRedemptions,
    t('customerAccounts.redemptions.error'),
  )

  return (
    <CustomerDataPageHeader customerId={customerId} sectionLabel={t('customerAccounts.sections.voucherRedemptions')} t={t}>
      <Card className="mt-5 rounded-xl border-border/80 shadow-none overflow-hidden">
        <CardContent className="p-0">
          {errorMessage ? (
            <div className="flex flex-wrap items-center justify-between gap-3 p-4 text-[13px] font-medium text-destructive">
              <p>{errorMessage}</p>
              <Button variant="outline" size="sm" onClick={retry}>
                {t('customerAccounts.retry')}
              </Button>
            </div>
          ) : (
            <>
              <CustomerVoucherRedemptionsTable
                redemptions={data}
                isLoading={isLoading}
                isRefreshing={isRefreshing}
                language={i18n.resolvedLanguage}
                t={t}
              />
              <div className="p-4 border-t border-border">
                <ListPagination
                  meta={meta}
                  page={page}
                  pageSize={pageSize}
                  onPageChange={handlePageChange}
                  onPageSizeChange={handlePageSizeChange}
                />
              </div>
            </>
          )}
        </CardContent>
      </Card>
    </CustomerDataPageHeader>
  )
}

export { CustomerVoucherRedemptionsPage }
