import { CaretRightIcon } from '@phosphor-icons/react'
import { Link } from 'react-router-dom'

function Breadcrumb({ items }) {
  return (
    <nav aria-label="Breadcrumb">
      <ol className="flex flex-wrap items-center gap-1.5 text-[12px] font-medium text-muted-foreground">
        {items.map((item, index) => {
          const isCurrent = index === items.length - 1

          return (
            <li key={`${item.label}-${index}`} className="flex min-w-0 items-center gap-1.5">
              {index > 0 ? <CaretRightIcon size={12} className="shrink-0 text-muted-foreground/60" /> : null}
              {isCurrent || !item.to ? (
                <span className={isCurrent ? 'truncate text-foreground' : 'truncate'} aria-current={isCurrent ? 'page' : undefined}>
                  {item.label}
                </span>
              ) : (
                <Link className="truncate transition-colors hover:text-foreground" to={item.to}>
                  {item.label}
                </Link>
              )}
            </li>
          )
        })}
      </ol>
    </nav>
  )
}

export { Breadcrumb }
