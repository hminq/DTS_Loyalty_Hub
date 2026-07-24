import { CircleNotchIcon } from '@phosphor-icons/react'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useLocation, useNavigate, useParams } from 'react-router-dom'

import { getCustomerVoucherDetail } from '../api/customerVouchersApi'
import { CustomerVoucherDetails } from '../components/customer-vouchers/CustomerVoucherDetails'
import { Breadcrumb } from '../components/layout/Breadcrumb'
import { PageHeader } from '../components/layout/PageHeader'
import { Button } from '../components/ui/button'

function CustomerVoucherDetailPage() {
  const { customerVoucherId } = useParams()
  const { i18n, t } = useTranslation()
  const location = useLocation()
  const navigate = useNavigate()
  const [voucher, setVoucher] = useState(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState('')
  const [refreshKey, setRefreshKey] = useState(0)

  const returnSearch = location.state?.returnSearch
  const listTarget = returnSearch ? `/vouchers/customer-vouchers?${returnSearch}` : '/vouchers/customer-vouchers'

  useEffect(() => {
    const controller = new AbortController()
    setIsLoading(true)
    setErrorMessage('')

    getCustomerVoucherDetail(customerVoucherId, controller.signal)
      .then((result) => {
        if (!controller.signal.aborted) setVoucher(result)
      })
      .catch((error) => {
        if (controller.signal.aborted) return

        if (error.code === 'CUSTOMER_VOUCHER_NOT_FOUND') {
          navigate(listTarget, {
            replace: true,
            state: { errorMessage: t('customerVouchers.errors.notFound') },
          })
          return
        }

        setErrorMessage(error.message || t('customerVouchers.detail.loadError'))
      })
      .finally(() => {
        if (!controller.signal.aborted) setIsLoading(false)
      })

    return () => controller.abort()
  }, [customerVoucherId, listTarget, navigate, refreshKey, t])

  return (
    <>
      <PageHeader
        breadcrumb={<Breadcrumb items={[
          { label: t('customerVouchers.title'), to: listTarget },
          { label: voucher?.voucherDefName || t('customerVouchers.detail.title') },
        ]} />}
        title={voucher?.voucherDefName || t('customerVouchers.detail.title')}
        description={voucher?.voucherDefDescription || t('customerVouchers.detail.description')}
      />

      {errorMessage ? (
        <div className="mt-5 flex flex-wrap items-center justify-between gap-3 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          <p>{errorMessage}</p>
          <Button variant="outline" size="sm" onClick={() => setRefreshKey((key) => key + 1)}>
            {t('common.retry')}
          </Button>
        </div>
      ) : null}

      {isLoading ? (
        <div className="mt-5 flex items-center gap-2 text-[13px] text-muted-foreground" aria-busy="true">
          <CircleNotchIcon className="animate-spin" aria-hidden="true" />
          {t('customerVouchers.detail.loading')}
        </div>
      ) : voucher ? (
        <CustomerVoucherDetails voucher={voucher} language={i18n.resolvedLanguage} t={t} />
      ) : null}
    </>
  )
}

export { CustomerVoucherDetailPage }
