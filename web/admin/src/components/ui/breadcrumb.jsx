import { CaretRightIcon, DotsThreeIcon } from '@phosphor-icons/react'
import { cloneElement } from 'react'

import { cn } from '../../lib/utils'

function Breadcrumb({ ...props }) {
  return <nav aria-label="breadcrumb" {...props} />
}

function BreadcrumbList({ className, ...props }) {
  return (
    <ol
      className={cn('flex flex-wrap items-center gap-1.5 text-xs text-muted-foreground', className)}
      {...props}
    />
  )
}

function BreadcrumbItem({ className, ...props }) {
  return <li className={cn('inline-flex min-w-0 items-center gap-1.5', className)} {...props} />
}

function BreadcrumbLink({ className, render, children, ...props }) {
  const linkClassName = cn('truncate transition-colors hover:text-foreground', className)

  if (render) {
    return cloneElement(render, { ...props, className: cn(linkClassName, render.props.className) }, children)
  }

  return <a className={linkClassName} {...props}>{children}</a>
}

function BreadcrumbPage({ className, ...props }) {
  return (
    <span
      role="link"
      aria-disabled="true"
      aria-current="page"
      className={cn('truncate font-medium text-foreground', className)}
      {...props}
    />
  )
}

function BreadcrumbSeparator({ children, className, ...props }) {
  return (
    <li role="presentation" aria-hidden="true" className={cn('text-muted-foreground/60 [&>svg]:size-3', className)} {...props}>
      {children ?? <CaretRightIcon />}
    </li>
  )
}

function BreadcrumbEllipsis({ className, ...props }) {
  return (
    <span role="presentation" aria-hidden="true" className={cn('flex size-8 items-center justify-center', className)} {...props}>
      <DotsThreeIcon />
      <span className="sr-only">More</span>
    </span>
  )
}

export {
  Breadcrumb,
  BreadcrumbEllipsis,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
}
