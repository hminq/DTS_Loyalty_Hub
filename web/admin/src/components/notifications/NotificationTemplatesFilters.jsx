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

      <select
        className="h-10 rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
        value={eventTypeCode}
        onChange={(e) => onEventTypeChange(e.target.value)}
        disabled={isLoadingEventTypes}
      >
        <option value="">{t('notifications.filters.allEventTypes', 'All Event Types')}</option>
        {eventTypes.map((type) => (
          <option key={type.eventTypeCode} value={type.eventTypeCode}>
            {type.displayName || type.eventTypeCode}
          </option>
        ))}
      </select>
    </div>
  )
}

export { NotificationTemplatesFilters }
