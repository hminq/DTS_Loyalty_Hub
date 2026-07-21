import { ArrowLeftIcon } from '@phosphor-icons/react'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'

import { createAdminAccount } from '../api/adminAccountsApi'
import { getAllRoleOptions } from '../api/rolesApi'
import { AdminAccountForm } from '../components/admin-accounts/AdminAccountForm'
import { PageHeader } from '../components/layout/PageHeader'
import { Button } from '../components/ui/button'

function CreateAdminAccountPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const [roleOptions, setRoleOptions] = useState([])
  const [isLoadingRoles, setIsLoadingRoles] = useState(true)
  const [roleOptionsError, setRoleOptionsError] = useState('')

  useEffect(() => {
    let isCurrent = true

    async function loadRoles() {
      try {
        const roles = await getAllRoleOptions()
        if (isCurrent) setRoleOptions(roles)
      } catch (error) {
        if (isCurrent) setRoleOptionsError(error.message || t('errors.loadRoleOptions'))
      } finally {
        if (isCurrent) setIsLoadingRoles(false)
      }
    }

    loadRoles()
    return () => { isCurrent = false }
  }, [t])

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
        eyebrow={t('adminAccounts.eyebrow')}
        title={t('adminAccounts.createTitle')}
        description={t('adminAccounts.createDescription')}
        actions={(
          <Button variant="outline" size="sm" onClick={() => navigate('/admin-accounts')}>
            <ArrowLeftIcon size={15} />
            {t('adminAccounts.detail.back')}
          </Button>
        )}
      />
      <AdminAccountForm
        onSubmit={handleSubmit}
        onCancel={() => navigate('/admin-accounts')}
        roleOptions={roleOptions}
        isLoadingRoles={isLoadingRoles}
        roleOptionsError={roleOptionsError}
        t={t}
      />
    </>
  )
}

export { CreateAdminAccountPage }
