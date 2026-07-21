import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'

import { Button } from '../ui/button'

function DeleteRoleDialog({ role, onClose, onConfirm }) {
  const { t } = useTranslation()
  const [isDeleting, setIsDeleting] = useState(false)
  const [errorMessage, setErrorMessage] = useState('')

  useEffect(() => {
    setErrorMessage('')
    setIsDeleting(false)
  }, [role])

  if (!role) return null

  async function handleConfirm() {
    setIsDeleting(true)
    setErrorMessage('')

    try {
      await onConfirm(role)
    } catch (error) {
      setErrorMessage(error.message || t('roles.delete.error'))
      setIsDeleting(false)
    }
  }

  return (
    <div className="fixed inset-0 z-50 grid place-items-center bg-foreground/45 px-4" role="presentation">
      <section
        className="w-full max-w-md rounded-xl border border-border bg-background p-5 shadow-2xl"
        role="alertdialog"
        aria-modal="true"
        aria-labelledby="delete-role-title"
        aria-describedby="delete-role-description"
      >
        <h2 id="delete-role-title" className="text-base font-semibold tracking-tight">{t('roles.delete.title')}</h2>
        <p id="delete-role-description" className="mt-2 text-[13px] leading-5 text-muted-foreground">
          {t('roles.delete.description', { name: role.name })}
        </p>
        {errorMessage ? (
          <p className="mt-4 rounded-lg border border-destructive/20 bg-destructive/5 px-3 py-2 text-[13px] font-medium text-destructive">
            {errorMessage}
          </p>
        ) : null}
        <div className="mt-5 flex justify-end gap-2">
          <Button variant="outline" onClick={onClose} disabled={isDeleting}>{t('common.cancel')}</Button>
          <Button variant="destructive" onClick={handleConfirm} disabled={isDeleting}>
            {isDeleting ? t('roles.delete.deleting') : t('roles.delete.confirm')}
          </Button>
        </div>
      </section>
    </div>
  )
}

export { DeleteRoleDialog }
