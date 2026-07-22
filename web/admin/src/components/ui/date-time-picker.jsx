import { CalendarBlankIcon } from '@phosphor-icons/react'
import { format } from 'date-fns'
import { DayPicker } from 'react-day-picker'

import { cn } from '../../lib/utils'
import { Button } from './button'
import { Input } from './input'
import { Popover, PopoverContent, PopoverTrigger } from './popover'

function DateTimePicker({ value, onChange, placeholder, clearLabel, locale }) {
  const selected = parseUtc(value)

  function selectDate(date) {
    if (!date) return
    const next = new Date(date)
    next.setHours(selected?.getHours() ?? 0, selected?.getMinutes() ?? 0, 0, 0)
    onChange(next.toISOString())
  }

  function changeTime(event) {
    if (!selected) return
    const [hours, minutes] = event.target.value.split(':').map(Number)
    const next = new Date(selected)
    next.setHours(hours, minutes, 0, 0)
    onChange(next.toISOString())
  }

  return (
    <Popover>
      <PopoverTrigger
        render={<Button
          variant="outline"
          className={cn('h-9 w-full justify-between px-3 text-[13px] font-normal', !selected && 'text-muted-foreground')}
        >
          <span className="truncate">{selected ? format(selected, 'dd/MM/yyyy, HH:mm') : placeholder}</span>
          <CalendarBlankIcon aria-hidden="true" />
        </Button>}
      />
      <PopoverContent className="w-auto p-3">
        <DayPicker
          mode="single"
          selected={selected}
          onSelect={selectDate}
          defaultMonth={selected}
          classNames={calendarClassNames}
        />
        <div className="mt-3 flex items-center gap-2 border-t border-border pt-3">
          <Input
            type="time"
            className="h-8 min-w-0 flex-1 text-xs"
            value={selected ? format(selected, 'HH:mm') : ''}
            onChange={changeTime}
            disabled={!selected}
          />
          {selected ? <Button variant="ghost" size="sm" onClick={() => onChange('')}>{clearLabel}</Button> : null}
        </div>
      </PopoverContent>
    </Popover>
  )
}

function parseUtc(value) {
  if (!value) return undefined
  const parsed = new Date(value)
  return Number.isNaN(parsed.getTime()) ? undefined : parsed
}

const calendarClassNames = {
  months: 'flex flex-col',
  month: 'space-y-3',
  month_caption: 'relative flex h-8 items-center justify-center',
  caption_label: 'text-sm font-medium',
  nav: 'absolute inset-x-0 top-0 flex h-8 items-center justify-between',
  button_previous: 'size-8 rounded-md text-muted-foreground hover:bg-muted',
  button_next: 'size-8 rounded-md text-muted-foreground hover:bg-muted',
  month_grid: 'border-collapse',
  weekdays: 'flex',
  weekday: 'w-9 text-center text-[11px] font-normal text-muted-foreground',
  week: 'mt-1 flex w-full',
  day: 'relative size-9 p-0 text-center text-sm',
  day_button: 'size-9 rounded-md text-sm hover:bg-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/30',
  selected: '[&_button]:bg-primary [&_button]:text-primary-foreground [&_button]:hover:bg-primary',
  today: '[&_button]:font-semibold [&_button]:text-primary',
  outside: 'text-muted-foreground opacity-40',
  disabled: 'text-muted-foreground opacity-30',
  hidden: 'invisible',
}

export { DateTimePicker }
