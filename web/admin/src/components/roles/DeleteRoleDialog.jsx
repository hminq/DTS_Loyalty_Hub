import { useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'

import { Button } from '../ui/button'

function DeleteRoleDialog({ role, onClose, onConfirm }) {
  const { t } = useTranslation()
  const dialogRef = useRef(null)
  const [isDeleting, setIsDeleting] = useState(false)
  const [errorMessage, setErrorMessage] = useState('')

  useEffect(() => {
    setErrorMessage('')
    setIsDeleting(false)

    const dialog = dialogRef.current
    if (!dialog) return

    if (role && !dialog.open) {
      dialog.showModal()
    } else if (!role && dialog.open) {
      dialog.close()
    }
  }, [role])

  async function handleConfirm() {
    if (!role) return

    setIsDeleting(true)
    setErrorMessage('')

    try {
      await onConfirm(role)
    } catch (error) {
      setErrorMessage(error.message || t('roles.delete.error'))
      setIsDeleting(false)
    }
  }

  function handleCancel(event) {
    event.preventDefault()
    if (!isDeleting) onClose()
  }

  function handleBackdropClick(event) {
    if (event.target === event.currentTarget && !isDeleting) {
      onClose()
    }
  }

  return (
    <dialog
      ref={dialogRef}
      className="m-auto w-[calc(100%-2rem)] max-w-md rounded-xl border border-border bg-background p-0 text-foreground shadow-2xl backdrop:bg-foreground/45"
      role="alertdialog"
      aria-labelledby="delete-role-title"
      aria-describedby="delete-role-description"
      onCancel={handleCancel}
      onClick={handleBackdropClick}
    >
      <section className="p-5">
        <h2 id="delete-role-title" className="text-base font-semibold tracking-tight">{t('roles.delete.title')}</h2>
        <p id="delete-role-description" className="mt-2 text-[13px] leading-5 text-muted-foreground">
          {t('roles.delete.description', { name: role?.name ?? '' })}
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
    </dialog>
  )
}

export { DeleteRoleDialog }
