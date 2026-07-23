import { useTranslation } from 'react-i18next'

import { AccountActionDialog } from '../accounts/AccountActionDialog'

function AdminAccountStatusDialog({ account, open, onClose, onConfirm }) {
  const { t } = useTranslation()
  const isDisabling = account?.status === 'ENABLE'

  return (
    <AccountActionDialog
      open={open}
      title={t(isDisabling
        ? 'adminAccounts.status.disableTitle'
        : 'adminAccounts.status.enableTitle')}
      description={t(isDisabling
        ? 'adminAccounts.status.disableDescription'
        : 'adminAccounts.status.enableDescription', {
        name: account?.fullName || account?.username || '',
      })}
      confirmLabel={t(isDisabling
        ? 'adminAccounts.status.disableConfirm'
        : 'adminAccounts.status.enableConfirm')}
      confirmingLabel={t('adminAccounts.status.updating')}
      errorFallback={t('adminAccounts.status.error')}
      confirmVariant={isDisabling ? 'destructive' : 'default'}
      cancelLabel={t('common.cancel')}
      onClose={onClose}
      onConfirm={() => onConfirm(isDisabling ? 'DISABLE' : 'ENABLE')}
    />
  )
}

export { AdminAccountStatusDialog }
