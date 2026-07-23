import { CircleNotchIcon } from '@phosphor-icons/react'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useOutletContext, useParams } from 'react-router-dom'

import { getAdminAccount, updateAdminAccount } from '../api/adminAccountsApi'
import { AdminAccountForm } from '../components/admin-accounts/AdminAccountForm'
import { Breadcrumb } from '../components/layout/Breadcrumb'
import { PageHeader } from '../components/layout/PageHeader'

function EditAdminAccountPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { adminId } = useParams()
  const { currentAdmin, refreshCurrentAdmin } = useOutletContext()
  const [account, setAccount] = useState(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState('')

  useEffect(() => {
    const controller = new AbortController()

    async function loadAccount() {
      try {
        const result = await getAdminAccount(adminId, controller.signal)
        if (!controller.signal.aborted) setAccount(result)
      } catch (error) {
        if (!controller.signal.aborted) setErrorMessage(error.message || t('errors.loadAdminAccount'))
      } finally {
        if (!controller.signal.aborted) setIsLoading(false)
      }
    }

    loadAccount()
    return () => controller.abort()
  }, [adminId, t])

  async function handleSubmit(payload) {
    const updatedAccount = await updateAdminAccount(adminId, payload)

    if (currentAdmin?.adminId === adminId) {
      await refreshCurrentAdmin()
    }

    navigate(`/admin-accounts/${updatedAccount.adminId}`, {
      replace: true,
      state: { successMessage: t('adminAccounts.edit.success') },
    })
  }

  return (
    <>
      <PageHeader
        breadcrumb={<Breadcrumb items={[
          { label: t('adminAccounts.title'), to: '/admin-accounts' },
          {
            label: account?.fullName || account?.username || t('adminAccounts.detail.titleFallback'),
            to: `/admin-accounts/${adminId}`,
          },
          { label: t('adminAccounts.edit.title') },
        ]} />}
        title={t('adminAccounts.edit.title')}
        description={t('adminAccounts.edit.description')}
      />

      {errorMessage ? (
        <p className="mt-5 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          {errorMessage}
        </p>
      ) : null}
      {isLoading ? (
        <div className="mt-5 flex items-center gap-2 text-[13px] text-muted-foreground">
          <CircleNotchIcon className="animate-spin" aria-hidden="true" />
          {t('adminAccounts.edit.loading')}
        </div>
      ) : account ? (
        <AdminAccountForm
          mode="edit"
          initialValues={{
            username: account.username,
            email: account.email,
            fullName: account.fullName ?? '',
            phoneNumber: account.phoneNumber ?? '',
            roleId: account.roleId,
            roleName: account.roleName,
          }}
          submitLabel={t('adminAccounts.edit.submit')}
          submittingLabel={t('adminAccounts.edit.submitting')}
          submitError={t('adminAccounts.edit.error')}
          onSubmit={handleSubmit}
          onCancel={() => navigate(`/admin-accounts/${adminId}`)}
          t={t}
        />
      ) : null}
    </>
  )
}

export { EditAdminAccountPage }
