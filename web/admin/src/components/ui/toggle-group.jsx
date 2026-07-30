import { Toggle } from '@base-ui/react/toggle'
import { ToggleGroup as ToggleGroupPrimitive } from '@base-ui/react/toggle-group'
import { cva } from 'class-variance-authority'
import { createContext, forwardRef, useContext } from 'react'

import { cn } from '../../lib/utils'

const toggleVariants = cva(
  'inline-flex h-9 items-center justify-center gap-2 rounded-md px-3 text-sm font-medium outline-none transition-colors hover:bg-muted hover:text-foreground focus-visible:ring-2 focus-visible:ring-ring/20 disabled:pointer-events-none disabled:opacity-50 data-[pressed]:bg-accent data-[pressed]:text-accent-foreground [&_svg]:pointer-events-none [&_svg]:size-4 [&_svg]:shrink-0',
  {
    variants: {
      variant: {
        default: 'bg-transparent',
        outline: 'border border-border bg-background shadow-xs',
      },
      size: {
        default: 'h-9 px-3',
        sm: 'h-8 px-2.5 text-xs',
      },
    },
    defaultVariants: {
      variant: 'default',
      size: 'default',
    },
  },
)

const ToggleGroupContext = createContext({
  variant: 'default',
  size: 'default',
})

const ToggleGroup = forwardRef(function ToggleGroup(
  { className, variant = 'default', size = 'default', children, ...props },
  ref,
) {
  return (
    <ToggleGroupPrimitive
      ref={ref}
      className={cn('flex w-fit items-center gap-2', className)}
      {...props}
    >
      <ToggleGroupContext.Provider value={{ variant, size }}>
        {children}
      </ToggleGroupContext.Provider>
    </ToggleGroupPrimitive>
  )
})

const ToggleGroupItem = forwardRef(function ToggleGroupItem(
  { className, variant, size, ...props },
  ref,
) {
  const context = useContext(ToggleGroupContext)

  return (
    <Toggle
      ref={ref}
      className={cn(
        toggleVariants({
          variant: variant || context.variant,
          size: size || context.size,
        }),
        className,
      )}
      {...props}
    />
  )
})

export { ToggleGroup, ToggleGroupItem }
