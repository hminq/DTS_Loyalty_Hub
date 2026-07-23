import { useTranslation } from 'react-i18next'

import { AccountActionDialog } from '../accounts/AccountActionDialog'

function RevokeAdminSessionDialog({ account, open, onClose, onConfirm }) {
  const { t } = useTranslation()

  return (
    <AccountActionDialog
      open={open}
      title={t('adminAccounts.revoke.title')}
      description={t('adminAccounts.revoke.description', {
        name: account?.fullName || account?.username || '',
      })}
      confirmLabel={t('adminAccounts.revoke.confirm')}
      confirmingLabel={t('adminAccounts.revoke.revoking')}
      errorFallback={t('adminAccounts.revoke.error')}
      confirmVariant="destructive"
      cancelLabel={t('common.cancel')}
      onClose={onClose}
      onConfirm={onConfirm}
    />
  )
}

export { RevokeAdminSessionDialog }
