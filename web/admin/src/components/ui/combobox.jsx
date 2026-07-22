import { Combobox as ComboboxPrimitive } from '@base-ui/react/combobox'
import { CaretDownIcon, CheckIcon, CircleNotchIcon, MagnifyingGlassIcon } from '@phosphor-icons/react'
import { useMemo, useState } from 'react'

import { cn } from '../../lib/utils'
import { Button } from './button'

const EMPTY_OPTION_VALUE = '__empty__'

function Combobox({
  value,
  selectedLabel,
  options,
  onValueChange,
  placeholder,
  searchPlaceholder,
  emptyText,
  emptyOptionLabel,
  loadingText,
  error,
  isLoading = false,
  disabled = false,
  invalid = false,
  ariaLabel,
  searchValue,
  onSearchChange,
  shouldFilter = true,
  onOpenChange,
}) {
  const [open, setOpen] = useState(false)
  const [internalSearch, setInternalSearch] = useState('')
  const search = searchValue ?? internalSearch
  const items = useMemo(() => {
    const nextItems = options.map((option) => ({ ...option, key: option.value }))
    return emptyOptionLabel
      ? [{ value: EMPTY_OPTION_VALUE, key: EMPTY_OPTION_VALUE, label: emptyOptionLabel, isEmptyOption: true }, ...nextItems]
      : nextItems
  }, [emptyOptionLabel, options])
  const selected = useMemo(() => options.find((option) => option.value === value) ?? null, [options, value])
  const selectedItem = selected
    ?? (!value && emptyOptionLabel ? items[0] : null)
    ?? (value && selectedLabel ? { value, key: value, label: selectedLabel } : null)

  function changeOpen(nextOpen) {
    setOpen(nextOpen)
    if (!nextOpen) {
      setInternalSearch('')
      onSearchChange?.('')
    }
    onOpenChange?.(nextOpen)
  }

  function changeSearch(nextSearch) {
    setInternalSearch(nextSearch)
    onSearchChange?.(nextSearch)
  }

  function select(nextItem) {
    if (!nextItem || nextItem.isEmptyOption) {
      onValueChange('', null)
    } else {
      onValueChange(nextItem.value, nextItem)
    }
    changeOpen(false)
  }

  return (
    <ComboboxPrimitive.Root
      items={items}
      value={selectedItem}
      inputValue={search}
      open={open}
      onOpenChange={changeOpen}
      onInputValueChange={changeSearch}
      onValueChange={select}
      itemToStringLabel={(item) => item?.label ?? ''}
      isItemEqualToValue={(item, selectedValue) => item?.value === selectedValue?.value}
      filter={shouldFilter ? undefined : null}
      autoComplete={shouldFilter ? 'list' : 'none'}
      disabled={disabled}
    >
      <ComboboxPrimitive.Trigger
        render={<Button
          variant="outline"
          role="combobox"
          aria-label={ariaLabel}
          aria-invalid={invalid}
          className={cn(
            'h-9 w-full justify-between px-3 text-[13px] font-normal',
            !value && 'text-muted-foreground',
            invalid && 'border-destructive',
          )}
        >
          <span className="truncate">{selected?.label ?? selectedLabel ?? placeholder}</span>
          <ComboboxPrimitive.Icon render={<CaretDownIcon aria-hidden="true" />} />
        </Button>}
      />

      <ComboboxPrimitive.Portal>
        <ComboboxPrimitive.Positioner align="start" sideOffset={6} className="z-50">
          <ComboboxPrimitive.Popup className="w-[var(--anchor-width)] min-w-56 rounded-lg border border-border bg-popover p-1.5 text-popover-foreground shadow-md outline-none">
            <ComboboxPrimitive.InputGroup className="relative border-b border-border pb-1.5">
              <MagnifyingGlassIcon className="absolute left-2.5 top-1/2 -translate-y-[calc(50%+3px)] text-muted-foreground" aria-hidden="true" />
              <ComboboxPrimitive.Input
                aria-label={ariaLabel}
                className="h-8 w-full rounded-md bg-transparent pl-8 pr-2 text-[13px] outline-none placeholder:text-muted-foreground"
                placeholder={searchPlaceholder}
              />
            </ComboboxPrimitive.InputGroup>

            <ComboboxPrimitive.List className="max-h-56 overflow-y-auto py-1">
              {isLoading ? <ComboboxState><CircleNotchIcon className="animate-spin" />{loadingText}</ComboboxState> : null}
              {!isLoading && error ? <ComboboxState className="text-destructive">{error}</ComboboxState> : null}
              {!isLoading && !error ? (
                <>
                  <ComboboxPrimitive.Empty className="px-3 py-5 text-center text-xs text-muted-foreground">
                    {emptyText}
                  </ComboboxPrimitive.Empty>
                  <ComboboxPrimitive.Collection>{(item, index) => (
                    <ComboboxPrimitive.Item
                      key={item.key}
                      value={item}
                      index={index}
                      className={({ selected: isSelected }) => cn(
                        'flex cursor-default items-center justify-between gap-2 rounded-sm px-2.5 py-2 text-[13px] outline-none data-[highlighted]:bg-muted',
                        isSelected && 'font-medium text-primary',
                      )}
                    >
                      <span className="truncate">{item.label}</span>
                      <ComboboxPrimitive.ItemIndicator render={<CheckIcon weight="bold" aria-hidden="true" />} />
                    </ComboboxPrimitive.Item>
                  )}</ComboboxPrimitive.Collection>
                </>
              ) : null}
            </ComboboxPrimitive.List>
          </ComboboxPrimitive.Popup>
        </ComboboxPrimitive.Positioner>
      </ComboboxPrimitive.Portal>
    </ComboboxPrimitive.Root>
  )
}

function ComboboxState({ className, children }) {
  return <div className={cn('flex items-center justify-center gap-2 px-3 py-5 text-center text-xs text-muted-foreground', className)}>{children}</div>
}

export { Combobox }
