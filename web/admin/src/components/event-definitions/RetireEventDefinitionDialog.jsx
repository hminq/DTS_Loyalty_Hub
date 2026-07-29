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

export function RetireEventDefinitionDialog({
  open = false,
  onOpenChange,
  mode = 'version', // 'version' | 'type'
  targetName = '',
  onConfirm,
  isSubmitting = false,
  error = null,
}) {
  const { t } = useTranslation()

  const isType = mode === 'type'
  const title = isType
    ? t('eventDefinitions.dialogs.retireTypeTitle', { name: targetName })
    : t('eventDefinitions.dialogs.retireVersionTitle', { version: targetName })

  const description = isType
    ? t('eventDefinitions.dialogs.retireTypeDescription', { name: targetName })
    : t('eventDefinitions.dialogs.retireVersionDescription', { version: targetName })

  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogTitle className="text-destructive">{title}</AlertDialogTitle>
        <AlertDialogDescription>{description}</AlertDialogDescription>

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
          <Button variant="destructive" onClick={onConfirm} disabled={isSubmitting}>
            {isSubmitting ? (
              <span className="inline-flex items-center gap-1.5">
                <CircleNotchIcon className="animate-spin" size={14} />
                {t('eventDefinitions.dialogs.retiring')}
              </span>
            ) : (
              t('eventDefinitions.dialogs.retireConfirm')
            )}
          </Button>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}
