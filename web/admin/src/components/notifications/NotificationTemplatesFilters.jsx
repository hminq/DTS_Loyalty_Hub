import { MagnifyingGlassIcon } from '@phosphor-icons/react'
import { Input } from '../ui/input'

function NotificationTemplatesFilters({ keyword, onKeywordChange, eventTypeCode, onEventTypeChange, eventTypes, isLoadingEventTypes, t }) {
  return (
    <div className="flex flex-wrap items-center gap-3 border-b border-border p-4">
      <div className="relative w-full max-w-[280px]">
        <MagnifyingGlassIcon
          className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground"
          size={16}
        />
        <Input
          className="pl-9"
          placeholder={t('notifications.filters.search', 'Search templates...')}
          value={keyword}
          onChange={(e) => onKeywordChange(e.target.value)}
        />
      </div>
    </div>
  )
}

export { NotificationTemplatesFilters }
