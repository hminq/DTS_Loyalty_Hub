import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'

import { createAdminAccount } from '../api/adminAccountsApi'
import { AdminAccountForm } from '../components/admin-accounts/AdminAccountForm'
import { Breadcrumb } from '../components/layout/Breadcrumb'
import { PageHeader } from '../components/layout/PageHeader'

function CreateAdminAccountPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()

  async function handleSubmit(payload) {
    const createdAccount = await createAdminAccount(payload)
    navigate(`/admin-accounts/${createdAccount.adminId}`, {
      replace: true,
      state: { successMessage: t('adminAccounts.createSuccess') },
    })
  }

  return (
    <>
      <PageHeader
        breadcrumb={<Breadcrumb items={[
          { label: t('adminAccounts.title'), to: '/admin-accounts' },
          { label: t('adminAccounts.createTitle') },
        ]} />}
        title={t('adminAccounts.createTitle')}
        description={t('adminAccounts.createDescription')}
      />
      <AdminAccountForm
        mode="create"
        submitLabel={t('adminAccounts.submit')}
        submittingLabel={t('adminAccounts.submitting')}
        submitError={t('adminAccounts.createError')}
        onSubmit={handleSubmit}
        onCancel={() => navigate('/admin-accounts')}
        t={t}
      />
    </>
  )
}

export { CreateAdminAccountPage }
