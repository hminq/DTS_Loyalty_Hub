import { FunnelSimpleIcon, XIcon } from '@phosphor-icons/react'

import { Badge } from '../ui/badge'
import { Button } from '../ui/button'
import { Popover, PopoverContent, PopoverTrigger } from '../ui/popover'

function FilterPopoverControls({
  activeFilters,
  children,
  clearAllLabel,
  filterButtonLabel,
  isOpen,
  onClearAll,
  onOpenChange,
  onRemoveFilter,
  removeFilterLabel,
}) {
  const hasActiveFilters = activeFilters.length > 0

  return (
    <div className="mb-4">
      <div className="flex flex-wrap items-center gap-2">
        <Popover open={isOpen} onOpenChange={onOpenChange}>
          <PopoverTrigger render={(
            <Button variant="outline" size="sm">
              <FunnelSimpleIcon data-icon="inline-start" aria-hidden="true" />
              {filterButtonLabel}
              {hasActiveFilters ? (
                <Badge variant="success" className="ml-1 px-1.5 py-0 text-[11px]">
                  {activeFilters.length}
                </Badge>
              ) : null}
            </Button>
          )}
          />
          <PopoverContent align="start" className="w-[min(calc(100vw-2rem),560px)] p-4">
            {children}
          </PopoverContent>
        </Popover>

        {hasActiveFilters ? (
          <Button type="button" variant="default" size="sm" onClick={onClearAll}>
            {clearAllLabel}
          </Button>
        ) : null}
      </div>

      {hasActiveFilters ? (
        <div className="mt-2 flex min-w-0 flex-wrap items-center gap-2">
          {activeFilters.map((filter) => (
            <Badge
              key={filter.key}
              variant="outline"
              className="inline-flex min-h-8 max-w-[220px] shrink-0 whitespace-nowrap bg-success-muted px-2.5 text-sm text-success sm:max-w-[260px] lg:max-w-[320px]"
            >
              <span className="shrink-0 text-muted-foreground">{filter.label}:</span>
              <span className="min-w-0 flex-1 truncate" title={filter.value}>
                {getFilterDisplayValue(filter)}
              </span>
              <button
                type="button"
                className="shrink-0 rounded-sm text-success hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                onClick={() => onRemoveFilter(filter.key)}
                aria-label={removeFilterLabel(filter.label)}
              >
                <XIcon aria-hidden="true" />
              </button>
            </Badge>
          ))}
        </div>
      ) : null}
    </div>
  )
}

function getFilterDisplayValue(filter) {
  if (!filter.isText) return filter.value

  return filter.value.length > 10 ? `${filter.value.slice(0, 10)}...` : filter.value
}

export { FilterPopoverControls }
