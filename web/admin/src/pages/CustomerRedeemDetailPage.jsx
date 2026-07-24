import { CircleNotchIcon } from '@phosphor-icons/react'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useLocation, useNavigate, useParams } from 'react-router-dom'

import { getCustomerRedeemDetail } from '../api/customerVouchersApi'
import { CustomerRedeemDetails } from '../components/customer-vouchers/CustomerRedeemDetails'
import { Breadcrumb } from '../components/layout/Breadcrumb'
import { PageHeader } from '../components/layout/PageHeader'
import { Button } from '../components/ui/button'

function CustomerRedeemDetailPage() {
  const { voucherRedemptionId } = useParams()
  const { i18n, t } = useTranslation()
  const location = useLocation()
  const navigate = useNavigate()
  const [redemption, setRedemption] = useState(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState('')
  const [refreshKey, setRefreshKey] = useState(0)

  const returnSearch = location.state?.returnSearch
  const listTarget = returnSearch ? `/vouchers/redemptions?${returnSearch}` : '/vouchers/redemptions'
  const title = redemption?.voucher?.name || t('voucherRedemptions.detail.title')

  useEffect(() => {
    const controller = new AbortController()
    setIsLoading(true)
    setErrorMessage('')

    getCustomerRedeemDetail(voucherRedemptionId, controller.signal)
      .then((result) => {
        if (!controller.signal.aborted) setRedemption(result)
      })
      .catch((error) => {
        if (controller.signal.aborted) return

        if (error.code === 'VOUCHER_REDEMPTION_NOT_FOUND') {
          navigate(listTarget, {
            replace: true,
            state: { errorMessage: t('voucherRedemptions.errors.notFound') },
          })
          return
        }

        setErrorMessage(error.message || t('voucherRedemptions.detail.loadError'))
      })
      .finally(() => {
        if (!controller.signal.aborted) setIsLoading(false)
      })

    return () => controller.abort()
  }, [voucherRedemptionId, listTarget, navigate, refreshKey, t])

  return (
    <>
      <PageHeader
        breadcrumb={<Breadcrumb items={[
          { label: t('voucherRedemptions.title'), to: listTarget },
          { label: title },
        ]} />}
        title={title}
        description={redemption?.voucher?.description || t('voucherRedemptions.detail.description')}
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
          {t('voucherRedemptions.detail.loading')}
        </div>
      ) : redemption ? (
        <CustomerRedeemDetails
          redemption={redemption}
          language={i18n.resolvedLanguage}
          t={t}
        />
      ) : null}
    </>
  )
}

export { CustomerRedeemDetailPage }
