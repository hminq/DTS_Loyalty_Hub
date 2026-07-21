import { CheckIcon, MinusIcon } from '@phosphor-icons/react'
import { useMemo } from 'react'

import { cn } from '../../lib/utils'

function PermissionMatrix({
  groups,
  selectedPermissionIds,
  onPermissionToggle,
  onPermissionGroupToggle,
  readOnly = false,
  labels,
}) {
  const matrix = useMemo(() => buildPermissionMatrix(groups), [groups])
  const selectedIds = useMemo(() => new Set(selectedPermissionIds ?? []), [selectedPermissionIds])
  const showsReadOnlySelection = readOnly && selectedPermissionIds !== undefined

  return (
    <div className="overflow-hidden rounded-xl border border-border">
      {readOnly ? (
        <div className="flex flex-wrap items-center gap-4 border-b border-border bg-muted/30 px-3 py-2 text-[11px] text-muted-foreground">
          <MatrixLegend state="selected" label={showsReadOnlySelection ? labels.assigned : labels.defined} />
          {showsReadOnlySelection ? <MatrixLegend state="unselected" label={labels.notAssigned} /> : null}
          <MatrixLegend state="missing" label={labels.notDefined} />
        </div>
      ) : null}

      <div className="overflow-x-auto">
        <table className="w-full min-w-max border-collapse text-left text-[13px]">
          <thead className="bg-muted/55 text-[11px] uppercase tracking-[0.1em] text-muted-foreground">
            <tr>
              <th className="sticky left-0 z-10 min-w-44 border-r border-border bg-muted px-3 py-2.5 font-semibold">
                {readOnly ? labels.group : (
                  <div className="flex items-center justify-between gap-3">
                    <span>{labels.group}</span>
                    <SelectAllButton
                      label={labels.all}
                      ariaLabel={labels.selectAll}
                      state={getSelectionState(matrix.allPermissionIds, selectedIds) === 'all' ? 'all' : 'none'}
                      onClick={() => onPermissionGroupToggle(matrix.allPermissionIds)}
                    />
                  </div>
                )}
              </th>
              {matrix.actions.map((action) => (
                <th key={action.actionCode} className="min-w-24 px-2 py-2.5 text-center font-semibold">
                  {action.actionName}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {matrix.rows.map((row) => (
              <tr key={row.groupCode} className="border-t border-border hover:bg-muted/20">
                <th className="sticky left-0 z-10 border-r border-border bg-card px-3 py-2.5 font-normal">
                  {readOnly ? (
                    <>
                      <p className="text-xs font-semibold text-foreground">{row.groupName}</p>
                      <p className="mt-0.5 font-mono text-[11px] text-muted-foreground">{row.groupCode}</p>
                    </>
                  ) : (
                    <SelectionButton
                      label={row.groupName}
                      secondaryLabel={row.groupCode}
                      ariaLabel={labels.selectGroup(row.groupName)}
                      state={getSelectionState(row.permissionIds, selectedIds)}
                      onClick={() => onPermissionGroupToggle(row.permissionIds)}
                    />
                  )}
                </th>
                {matrix.actions.map((action) => {
                  const permission = row.permissionsByAction.get(action.actionCode)

                  return (
                    <td key={action.actionCode} className="px-2 py-2.5 text-center">
                      <PermissionBox
                        permission={permission}
                        selected={permission ? selectedIds.has(permission.permissionId) : false}
                        readOnly={readOnly}
                        showsReadOnlySelection={showsReadOnlySelection}
                        onToggle={onPermissionToggle}
                        labels={labels}
                      />
                    </td>
                  )
                })}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}

function SelectAllButton({ label, ariaLabel, state, onClick }) {
  return (
    <button
      type="button"
      className="inline-flex shrink-0 items-center gap-1.5 rounded-md px-1.5 py-1 text-[11px] font-semibold normal-case tracking-normal text-foreground hover:bg-background"
      aria-label={ariaLabel}
      aria-pressed={state === 'all'}
      onClick={onClick}
    >
      <SelectionIndicator state={state} />
      {label}
    </button>
  )
}

function SelectionButton({ label, secondaryLabel, ariaLabel, state, onClick }) {
  return (
    <button
      type="button"
      className="flex w-full items-center gap-2 text-left"
      aria-label={ariaLabel}
      aria-pressed={state === 'all'}
      onClick={onClick}
    >
      <SelectionIndicator state={state} />
      <span className="min-w-0">
        <span className="block truncate text-xs font-semibold text-foreground">{label}</span>
        {secondaryLabel ? <span className="mt-0.5 block truncate font-mono text-[11px] font-normal text-muted-foreground">{secondaryLabel}</span> : null}
      </span>
    </button>
  )
}

function SelectionIndicator({ state }) {
  const selected = state === 'all'
  const partial = state === 'partial'

  return (
    <span className={cn(
      'inline-grid size-5 shrink-0 place-items-center rounded border transition-colors',
      selected || partial
        ? 'border-primary bg-primary text-primary-foreground'
        : 'border-border bg-background text-muted-foreground',
    )}>
      {selected ? <CheckIcon size={11} weight="bold" /> : null}
      {partial ? <MinusIcon size={10} weight="bold" /> : null}
    </span>
  )
}

function getSelectionState(permissionIds, selectedIds) {
  if (permissionIds.length === 0) return 'none'

  const selectedCount = permissionIds.reduce(
    (count, permissionId) => count + (selectedIds.has(permissionId) ? 1 : 0),
    0,
  )

  if (selectedCount === permissionIds.length) return 'all'
  if (selectedCount > 0) return 'partial'
  return 'none'
}

function PermissionBox({ permission, selected, readOnly, showsReadOnlySelection, onToggle, labels }) {
  if (!permission) {
    return (
      <span
        className="inline-grid size-6 place-items-center rounded border border-dashed border-border bg-muted/40 text-muted-foreground/50"
        title={labels.notDefined}
        aria-label={labels.notDefined}
      >
        <MinusIcon size={11} />
      </span>
    )
  }

  const label = `${permission.name} · ${permission.code}`
  const stateClassName = selected
    ? 'border-primary bg-primary text-primary-foreground shadow-xs'
    : 'border-border bg-background text-muted-foreground hover:border-primary/50 hover:text-primary'

  if (readOnly) {
    const isChecked = !showsReadOnlySelection || selected

    return (
      <span
        className={cn(
          'inline-grid size-6 place-items-center rounded border',
          isChecked
            ? 'border-primary bg-primary text-primary-foreground shadow-xs'
            : 'border-border bg-background text-muted-foreground',
        )}
        title={label}
        aria-label={`${isChecked ? (labels.assigned ?? labels.defined) : labels.notAssigned}: ${permission.name}`}
      >
        {isChecked ? <CheckIcon size={13} weight="bold" /> : null}
      </span>
    )
  }

  return (
    <button
      type="button"
      className={cn('inline-grid size-6 place-items-center rounded border transition-colors', stateClassName)}
      title={label}
      aria-label={permission.name}
      aria-pressed={selected}
      onClick={() => onToggle(permission.permissionId)}
    >
      {selected ? <CheckIcon size={13} weight="bold" /> : null}
    </button>
  )
}

function MatrixLegend({ state, label }) {
  const isSelected = state === 'selected'
  const isMissing = state === 'missing'

  return (
    <span className="inline-flex items-center gap-2">
      <span className={cn(
        'inline-grid size-4 place-items-center rounded border',
        isSelected && 'border-primary bg-primary text-primary-foreground',
        state === 'unselected' && 'border-border bg-background text-muted-foreground',
        isMissing && 'border-dashed border-border bg-muted/40 text-muted-foreground/50',
      )}>
        {isSelected ? <CheckIcon size={9} weight="bold" /> : null}
        {isMissing ? <MinusIcon size={8} /> : null}
      </span>
      {label}
    </span>
  )
}

function buildPermissionMatrix(groups) {
  const actionsByCode = new Map()

  groups.forEach((group) => {
    ;(group.permissions ?? []).forEach((permission) => {
      const actionCode = permission.actionCode
      const candidate = {
        actionCode,
        actionName: permission.actionName,
        actionSortOrder: permission.actionSortOrder ?? Number.MAX_SAFE_INTEGER,
      }
      const current = actionsByCode.get(actionCode)

      if (!current || compareActionMetadata(candidate, current) < 0) {
        actionsByCode.set(actionCode, candidate)
      }
    })
  })

  const actions = [...actionsByCode.values()].sort(compareActionMetadata)

  const rows = groups.map((group) => ({
    ...group,
    permissionIds: (group.permissions ?? []).map((permission) => permission.permissionId),
    permissionsByAction: new Map(
      (group.permissions ?? []).map((permission) => [permission.actionCode, permission]),
    ),
  }))

  return {
    actions,
    rows,
    allPermissionIds: rows.flatMap((row) => row.permissionIds),
  }
}

function compareActionMetadata(left, right) {
  const orderDifference = left.actionSortOrder - right.actionSortOrder
  if (orderDifference !== 0) return orderDifference

  const codeDifference = left.actionCode.localeCompare(right.actionCode)
  if (codeDifference !== 0) return codeDifference

  return left.actionName.localeCompare(right.actionName)
}

export { PermissionMatrix }
