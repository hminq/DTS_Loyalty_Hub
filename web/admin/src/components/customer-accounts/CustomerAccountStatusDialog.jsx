import { useTranslation } from 'react-i18next'

import { AccountActionDialog } from '../accounts/AccountActionDialog'

function CustomerAccountStatusDialog({ account, open, onClose, onConfirm }) {
  const { t } = useTranslation()
  const isDisabling = account?.status === 'ENABLE'

  return (
    <AccountActionDialog
      open={open}
      title={t(isDisabling
        ? 'customerAccounts.status.disableTitle'
        : 'customerAccounts.status.enableTitle')}
      description={t(isDisabling
        ? 'customerAccounts.status.disableDescription'
        : 'customerAccounts.status.enableDescription', {
        name: account?.fullName || account?.username || '',
      })}
      confirmLabel={t(isDisabling
        ? 'customerAccounts.status.disableConfirm'
        : 'customerAccounts.status.enableConfirm')}
      confirmingLabel={t('customerAccounts.status.updating')}
      errorFallback={t('customerAccounts.status.error')}
      confirmVariant={isDisabling ? 'destructive' : 'default'}
      cancelLabel={t('common.cancel')}
      onClose={onClose}
      onConfirm={() => onConfirm(isDisabling ? 'DISABLE' : 'ENABLE')}
    />
  )
}

export { CustomerAccountStatusDialog }
