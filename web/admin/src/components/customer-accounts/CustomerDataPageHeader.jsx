import { CircleNotchIcon } from '@phosphor-icons/react'
import { useEffect, useState } from 'react'
import { useNavigate, useOutletContext } from 'react-router-dom'

import { getCustomerAccount } from '../../api/customerAccountsApi'
import { Breadcrumb } from '../layout/Breadcrumb'
import { PageHeader } from '../layout/PageHeader'
import { Button } from '../ui/button'
import { PermissionCodes } from '../../constants/permissionCodes'
import { CustomerSectionNavigation } from './CustomerSectionNavigation'

function CustomerDataPageHeader({ customerId, sectionLabel, t, children }) {
  const navigate = useNavigate()
  const outletContext = useOutletContext()
  const hasPermission = outletContext?.hasPermission || (() => false)
  const [account, setAccount] = useState(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState('')

  const canEdit = hasPermission(PermissionCodes.CustomerUsers.Update)

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
  }, [customerId, t])

  if (errorMessage) {
    return (
      <>
        <PageHeader
          breadcrumb={<Breadcrumb items={[
            { label: t('customerAccounts.title'), to: '/customer-accounts' },
            { label: sectionLabel },
          ]} />}
          title={t('customerAccounts.detail.titleFallback')}
        />
        <div className="mt-5 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          <p>{errorMessage}</p>
        </div>
      </>
    )
  }

  return (
    <>
      <PageHeader
        breadcrumb={<Breadcrumb items={[
          { label: t('customerAccounts.title'), to: '/customer-accounts' },
          { label: account?.fullName || account?.username || t('customerAccounts.detail.titleFallback'), to: `/customer-accounts/${customerId}` },
          { label: sectionLabel },
        ]} />}
        title={account?.fullName || account?.username || t('customerAccounts.detail.titleFallback')}
        description={account ? `@${account.username}` : undefined}
        actions={canEdit && account ? (
          <Button variant="outline" size="sm" onClick={() => navigate(`/customer-accounts/${customerId}/edit`)}>
            {t('customerAccounts.actions.edit')}
          </Button>
        ) : null}
      />

      <CustomerSectionNavigation customerId={customerId} t={t} />

      {isLoading ? (
        <div className="mt-5 flex items-center gap-2 text-[13px] text-muted-foreground" aria-busy="true">
          <CircleNotchIcon className="animate-spin" size={16} aria-hidden="true" />
          {t('customerAccounts.detail.loading')}
        </div>
      ) : (
        children
      )}
    </>
  )
}

export { CustomerDataPageHeader }
