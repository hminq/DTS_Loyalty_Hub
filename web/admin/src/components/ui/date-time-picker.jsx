import { CalendarBlankIcon } from '@phosphor-icons/react'
import { format } from 'date-fns'

import { cn } from '../../lib/utils'
import { Button } from './button'
import { Calendar } from './calendar'
import { Input } from './input'
import { Popover, PopoverContent, PopoverTrigger } from './popover'

function DateTimePicker({ value, onChange, placeholder, clearLabel, minDateTime, disabled }) {
  const selected = parseUtc(value)

  function selectDate(date) {
    if (!date) return
    const next = new Date(date)
    next.setHours(selected?.getHours() ?? 0, selected?.getMinutes() ?? 0, 0, 0)

    if (minDateTime && isSameDay(next, minDateTime)) {
      const minTimeMinutes = minDateTime.getHours() * 60 + minDateTime.getMinutes()
      const nextMinutes = next.getHours() * 60 + next.getMinutes()
      if (nextMinutes < minTimeMinutes) {
        next.setHours(minDateTime.getHours(), minDateTime.getMinutes(), 0, 0)
      }
    }

    onChange(next.toISOString())
  }

  function changeTime(event) {
    if (!selected || !event.target.value) return
    const [hours, minutes] = event.target.value.split(':').map(Number)
    const next = new Date(selected)
    next.setHours(hours, minutes, 0, 0)

    if (minDateTime && isSameDay(next, minDateTime) && next < minDateTime) {
      next.setHours(minDateTime.getHours(), minDateTime.getMinutes(), 0, 0)
    }

    onChange(next.toISOString())
  }

  const isSelectedSameAsMin = selected && minDateTime && isSameDay(selected, minDateTime)
  const minTime = isSelectedSameAsMin ? format(minDateTime, 'HH:mm') : undefined

  return (
    <Popover>
      <PopoverTrigger
        render={<Button
          variant="outline"
          disabled={disabled}
          className={cn('h-9 w-full justify-between px-3 text-[13px] font-normal', !selected && 'text-muted-foreground')}
        >
          <span className="truncate">{selected ? format(selected, 'dd/MM/yyyy, HH:mm') : placeholder}</span>
          <CalendarBlankIcon aria-hidden="true" />
        </Button>}
      />
      <PopoverContent className="w-auto p-0">
        <Calendar
          mode="single"
          selected={selected}
          onSelect={selectDate}
          defaultMonth={selected || minDateTime}
          disabled={minDateTime ? [{ before: new Date(minDateTime.getFullYear(), minDateTime.getMonth(), minDateTime.getDate()) }] : undefined}
        />
        <div className="mx-3 mb-3 flex items-center gap-2 border-t border-border pt-3">
          <Input
            type="time"
            className="h-8 min-w-0 flex-1 text-xs"
            value={selected ? format(selected, 'HH:mm') : ''}
            onChange={changeTime}
            min={minTime}
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

function isSameDay(d1, d2) {
  return (
    d1.getDate() === d2.getDate()
    && d1.getMonth() === d2.getMonth()
    && d1.getFullYear() === d2.getFullYear()
  )
}

export { DateTimePicker }
