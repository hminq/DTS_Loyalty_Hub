import { cn } from '../../lib/utils'

function FieldGroup({ className, ...props }) {
  return <div className={cn('grid gap-4', className)} {...props} />
}

function Field({ className, invalid = false, disabled = false, ...props }) {
  return (
    <div
      className={cn('flex min-w-0 flex-col gap-1.5', className)}
      data-invalid={invalid || undefined}
      data-disabled={disabled || undefined}
      {...props}
    />
  )
}

function FieldLabel({ className, ...props }) {
  return <label className={cn('text-xs font-medium', className)} {...props} />
}

function FieldDescription({ className, ...props }) {
  return <p className={cn('text-xs text-muted-foreground', className)} {...props} />
}

function FieldError({ className, ...props }) {
  return <p className={cn('text-xs font-normal text-destructive', className)} {...props} />
}

export { Field, FieldDescription, FieldError, FieldGroup, FieldLabel }
