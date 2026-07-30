import { CheckIcon } from '@phosphor-icons/react'
import { forwardRef } from 'react'

import { cn } from '../../lib/utils'

const Checkbox = forwardRef(function Checkbox({ className, checked, onChange, disabled, ...props }, ref) {
  return (
    <span className={cn('relative inline-flex size-4 shrink-0 items-center justify-center', disabled && 'opacity-50', className)}>
      <input
        type="checkbox"
        ref={ref}
        checked={checked}
        onChange={onChange}
        disabled={disabled}
        className="peer absolute inset-0 size-full cursor-pointer opacity-0 disabled:cursor-not-allowed"
        {...props}
      />
      <span
        aria-hidden="true"
        className={cn(
          'pointer-events-none inline-flex size-4 items-center justify-center rounded border border-input bg-background text-primary-foreground transition-colors',
          checked && 'border-primary bg-primary',
          !disabled && 'peer-focus-visible:ring-2 peer-focus-visible:ring-ring/20',
        )}
      >
        {checked ? <CheckIcon weight="bold" /> : null}
      </span>
    </span>
  )
})

export { Checkbox }
