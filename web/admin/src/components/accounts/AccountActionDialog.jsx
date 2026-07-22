import { useEffect, useState } from 'react'

import {
  AlertDialog,
  AlertDialogClose,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogTitle,
} from '../ui/alert-dialog'
import { Button } from '../ui/button'

function AccountActionDialog({
  open,
  title,
  description,
  confirmLabel,
  confirmingLabel,
  errorFallback,
  confirmVariant = 'default',
  cancelLabel,
  onClose,
  onConfirm,
}) {
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errorMessage, setErrorMessage] = useState('')

  useEffect(() => {
    if (open) {
      setIsSubmitting(false)
      setErrorMessage('')
    }
  }, [open])

  async function handleConfirm() {
    setIsSubmitting(true)
    setErrorMessage('')

    try {
      await onConfirm()
    } catch (error) {
      setErrorMessage(error.message || errorFallback)
      setIsSubmitting(false)
    }
  }

  return (
    <AlertDialog
      open={open}
      onOpenChange={(nextOpen) => {
        if (!nextOpen && !isSubmitting) onClose()
      }}
    >
      <AlertDialogContent>
        <AlertDialogTitle>{title}</AlertDialogTitle>
        <AlertDialogDescription>{description}</AlertDialogDescription>
        {errorMessage ? (
          <p className="mt-4 rounded-lg border border-destructive/20 bg-destructive/5 px-3 py-2 text-[13px] font-medium text-destructive">
            {errorMessage}
          </p>
        ) : null}
        <AlertDialogFooter>
          <AlertDialogClose
            render={<Button variant="outline" disabled={isSubmitting}>{cancelLabel}</Button>}
          />
          <Button variant={confirmVariant} onClick={handleConfirm} disabled={isSubmitting}>
            {isSubmitting ? confirmingLabel : confirmLabel}
          </Button>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}

export { AccountActionDialog }
