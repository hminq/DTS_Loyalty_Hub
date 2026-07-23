import { CircleNotchIcon } from '@phosphor-icons/react'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useParams } from 'react-router-dom'

import { getCustomerAccount, updateCustomerAccount } from '../api/customerAccountsApi'
import { CustomerAccountForm } from '../components/customer-accounts/CustomerAccountForm'
import { Breadcrumb } from '../components/layout/Breadcrumb'
import { PageHeader } from '../components/layout/PageHeader'

function EditCustomerAccountPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { customerId } = useParams()
  const [account, setAccount] = useState(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState('')

  useEffect(() => {
    const controller = new AbortController()

    async function loadAccount() {
      try {
        const result = await getCustomerAccount(customerId, controller.signal)
        if (!controller.signal.aborted) setAccount(result)
      } catch (error) {
        if (controller.signal.aborted) return
        if (error.status === 403) setErrorMessage(t('customerAccounts.detail.forbidden'))
        else if (error.code === 'CUSTOMER_USER_NOT_FOUND') setErrorMessage(t('customerAccounts.detail.notFound'))
        else setErrorMessage(error.message || t('errors.loadCustomerAccount'))
      } finally {
        if (!controller.signal.aborted) setIsLoading(false)
      }
    }

    loadAccount()
    return () => controller.abort()
  }, [customerId, t])

  async function handleSubmit(payload) {
    const updatedAccount = await updateCustomerAccount(customerId, payload)

    navigate(`/customer-accounts/${updatedAccount.customerId}`, {
      replace: true,
      state: { successMessage: t('customerAccounts.edit.success') },
    })
  }

  return (
    <>
      <PageHeader
        breadcrumb={<Breadcrumb items={[
          { label: t('customerAccounts.title'), to: '/customer-accounts' },
          {
            label: account?.fullName || account?.username || t('customerAccounts.detail.titleFallback'),
            to: `/customer-accounts/${customerId}`,
          },
          { label: t('customerAccounts.edit.title') },
        ]} />}
        title={t('customerAccounts.edit.title')}
        description={t('customerAccounts.edit.description')}
      />

      {errorMessage ? (
        <p className="mt-5 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          {errorMessage}
        </p>
      ) : null}
      {isLoading ? (
        <div className="mt-5 flex items-center gap-2 text-[13px] text-muted-foreground" aria-busy="true">
          <CircleNotchIcon className="animate-spin" aria-hidden="true" size={16} />
          {t('customerAccounts.edit.loading')}
        </div>
      ) : account ? (
        <CustomerAccountForm
          initialValues={{
            username: account.username,
            email: account.email,
            fullName: account.fullName ?? '',
            phoneNumber: account.phoneNumber ?? '',
            tierName: account.tierName ?? '',
            status: account.status,
          }}
          submitLabel={t('customerAccounts.edit.submit')}
          submittingLabel={t('customerAccounts.edit.submitting')}
          submitError={t('customerAccounts.edit.error')}
          onSubmit={handleSubmit}
          onCancel={() => navigate(`/customer-accounts/${customerId}`)}
          t={t}
        />
      ) : null}
    </>
  )
}

export { EditCustomerAccountPage }
