import { CircleNotchIcon } from '@phosphor-icons/react'
import { useTranslation } from 'react-i18next'

import {
  AlertDialog,
  AlertDialogClose,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogTitle,
} from '../ui/alert-dialog'
import { Button } from '../ui/button'

export function PublishEventDefinitionDialog({
  open = false,
  onOpenChange,
  versionNumber,
  onConfirm,
  isSubmitting = false,
  error = null,
}) {
  const { t } = useTranslation()

  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogTitle>{t('eventDefinitions.dialogs.publishTitle', { version: versionNumber })}</AlertDialogTitle>
        <AlertDialogDescription>
          {t('eventDefinitions.dialogs.publishDescription', { version: versionNumber })}
        </AlertDialogDescription>

        {error && (
          <div className="mt-3 rounded-md bg-destructive/10 p-3 text-xs font-medium text-destructive">
            {error}
          </div>
        )}

        <AlertDialogFooter>
          <AlertDialogClose asChild>
            <Button variant="outline" disabled={isSubmitting}>
              {t('common.cancel')}
            </Button>
          </AlertDialogClose>
          <Button onClick={onConfirm} disabled={isSubmitting}>
            {isSubmitting ? (
              <span className="inline-flex items-center gap-1.5">
                <CircleNotchIcon className="animate-spin" size={14} />
                {t('eventDefinitions.dialogs.publishing')}
              </span>
            ) : (
              t('eventDefinitions.dialogs.publishConfirm')
            )}
          </Button>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}
