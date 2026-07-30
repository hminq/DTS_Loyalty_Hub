import { cva } from 'class-variance-authority'

import { cn } from '../../lib/utils'

const alertVariants = cva(
  'relative grid w-full grid-cols-[0_minmax(0,1fr)] items-start gap-y-1 rounded-md border px-4 py-3 text-sm has-[>svg]:grid-cols-[1.125rem_minmax(0,1fr)] has-[>svg]:gap-x-3 [&>svg]:size-[1.125rem] [&>svg]:translate-y-px',
  {
    variants: {
      variant: {
        default: 'bg-card text-card-foreground',
        warning: 'border-warning/30 bg-warning-muted text-foreground [&>svg]:text-warning',
        destructive: 'bg-card text-destructive [&>svg]:text-current',
      },
    },
    defaultVariants: {
      variant: 'default',
    },
  },
)

function Alert({ className, variant, ...props }) {
  return (
    <div
      role="alert"
      className={cn(alertVariants({ variant }), className)}
      {...props}
    />
  )
}

function AlertTitle({ className, ...props }) {
  return (
    <div
      className={cn('col-start-2 min-h-5 font-medium leading-5', className)}
      {...props}
    />
  )
}

function AlertDescription({ className, ...props }) {
  return (
    <div
      className={cn('col-start-2 text-sm leading-5 text-muted-foreground', className)}
      {...props}
    />
  )
}

export { Alert, AlertDescription, AlertTitle }
