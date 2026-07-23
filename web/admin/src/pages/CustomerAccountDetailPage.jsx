import { CircleNotchIcon } from '@phosphor-icons/react'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useLocation, useNavigate, useOutletContext, useParams } from 'react-router-dom'

import { getCustomerAccount } from '../api/customerAccountsApi'
import { CustomerAccountDetails } from '../components/customer-accounts/CustomerAccountDetails'
import { CustomerSectionNavigation } from '../components/customer-accounts/CustomerSectionNavigation'
import { Breadcrumb } from '../components/layout/Breadcrumb'
import { PageHeader } from '../components/layout/PageHeader'
import { Button } from '../components/ui/button'
import { PermissionCodes } from '../constants/permissionCodes'

function CustomerAccountDetailPage() {
  const { customerId } = useParams()
  const { i18n, t } = useTranslation()
  const navigate = useNavigate()
  const location = useLocation()
  const outletContext = useOutletContext()
  const hasPermission = outletContext?.hasPermission || (() => false)
  const [account, setAccount] = useState(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState('')
  const [successMessage, setSuccessMessage] = useState('')
  const [refreshKey, setRefreshKey] = useState(0)

  const canEdit = hasPermission(PermissionCodes.CustomerUsers.Update)

  useEffect(() => {
    if (location.state?.successMessage) {
      setSuccessMessage(location.state.successMessage)
      window.history.replaceState({}, document.title)
    }
  }, [location.state])

  useEffect(() => {
    const controller = new AbortController()
    setIsLoading(true)
    setErrorMessage('')
    setAccount(null)

    getCustomerAccount(customerId, controller.signal)
      .then((result) => {
        if (!controller.signal.aborted) setAccount(result)
      })
      .catch((error) => {
        if (controller.signal.aborted) return

        if (error.status === 403) setErrorMessage(t('customerAccounts.detail.forbidden'))
        else if (error.code === 'CUSTOMER_USER_NOT_FOUND') setErrorMessage(t('customerAccounts.detail.notFound'))
        else setErrorMessage(error.message || t('errors.loadCustomerAccount'))
      })
      .finally(() => {
        if (!controller.signal.aborted) setIsLoading(false)
      })

    return () => controller.abort()
  }, [customerId, refreshKey, t])

  return (
    <>
      <PageHeader
        breadcrumb={<Breadcrumb items={[
          { label: t('customerAccounts.title'), to: '/customer-accounts' },
          { label: account?.fullName || account?.username || t('customerAccounts.detail.titleFallback') },
        ]} />}
        title={account?.fullName || account?.username || t('customerAccounts.detail.titleFallback')}
        description={account ? `@${account.username}` : t('customerAccounts.detail.description')}
        actions={canEdit && account ? (
          <Button variant="outline" size="sm" onClick={() => navigate(`/customer-accounts/${customerId}/edit`)}>
            {t('customerAccounts.actions.edit')}
          </Button>
        ) : null}
      />

      <CustomerSectionNavigation customerId={customerId} t={t} />

      {errorMessage ? (
        <div className="mt-5 flex flex-wrap items-center justify-between gap-3 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          <p>{errorMessage}</p>
          <Button
            variant="outline"
            size="sm"
            onClick={() => setRefreshKey((current) => current + 1)}
          >
            {t('customerAccounts.retry')}
          </Button>
        </div>
      ) : null}
      {successMessage ? (
        <p className="mt-5 rounded-lg border border-success/20 bg-success-muted px-4 py-3 text-[13px] font-medium text-success">
          {successMessage}
        </p>
      ) : null}

      {isLoading ? (
        <div className="mt-5 flex items-center gap-2 text-[13px] text-muted-foreground" aria-busy="true">
          <CircleNotchIcon className="animate-spin" size={16} aria-hidden="true" />
          {t('customerAccounts.detail.loading')}
        </div>
      ) : account ? (
        <CustomerAccountDetails account={account} language={i18n.resolvedLanguage} t={t} />
      ) : null}
    </>
  )
}

export { CustomerAccountDetailPage }
