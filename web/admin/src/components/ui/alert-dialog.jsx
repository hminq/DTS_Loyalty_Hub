import { AlertDialog as AlertDialogPrimitive } from '@base-ui/react/alert-dialog'

import { cn } from '../../lib/utils'

const AlertDialog = AlertDialogPrimitive.Root
const AlertDialogTrigger = AlertDialogPrimitive.Trigger
const AlertDialogClose = AlertDialogPrimitive.Close

function AlertDialogContent({ className, children, ...props }) {
  return (
    <AlertDialogPrimitive.Portal>
      <AlertDialogPrimitive.Backdrop className="fixed inset-0 z-50 bg-foreground/45" />
      <AlertDialogPrimitive.Viewport className="fixed inset-0 z-50 grid place-items-center overflow-y-auto p-4">
        <AlertDialogPrimitive.Popup
          className={cn(
            'w-full max-w-md rounded-xl border border-border bg-background p-5 text-foreground shadow-2xl outline-none',
            className,
          )}
          {...props}
        >
          {children}
        </AlertDialogPrimitive.Popup>
      </AlertDialogPrimitive.Viewport>
    </AlertDialogPrimitive.Portal>
  )
}

function AlertDialogTitle({ className, ...props }) {
  return (
    <AlertDialogPrimitive.Title
      className={cn('text-base font-semibold tracking-tight', className)}
      {...props}
    />
  )
}

function AlertDialogDescription({ className, ...props }) {
  return (
    <AlertDialogPrimitive.Description
      className={cn('mt-2 text-[13px] leading-5 text-muted-foreground', className)}
      {...props}
    />
  )
}

function AlertDialogFooter({ className, ...props }) {
  return <div className={cn('mt-5 flex justify-end gap-2', className)} {...props} />
}

export {
  AlertDialog,
  AlertDialogClose,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogTitle,
  AlertDialogTrigger,
}
