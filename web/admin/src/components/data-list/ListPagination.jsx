import { CaretLeftIcon, CaretRightIcon } from '@phosphor-icons/react'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'

import { Button } from '../ui/button'
import { Input } from '../ui/input'

function ListPagination({ meta, onPageChange, onPageSizeChange }) {
  const { t } = useTranslation()
  const totalPages = Math.max(meta?.totalPages ?? 0, 1)
  const page = meta?.page ?? 1
  const currentPageSize = meta?.pageSize ?? 20
  const [pageSizeInput, setPageSizeInput] = useState(String(currentPageSize))

  useEffect(() => {
    setPageSizeInput(String(currentPageSize))
  }, [currentPageSize])

  function commitPageSize() {
    const parsedPageSize = Number.parseInt(pageSizeInput, 10)

    if (!Number.isFinite(parsedPageSize)) {
      setPageSizeInput(String(currentPageSize))
      return
    }

    const nextPageSize = Math.min(Math.max(parsedPageSize, 1), 100)
    setPageSizeInput(String(nextPageSize))

    if (nextPageSize !== currentPageSize) {
      onPageSizeChange(nextPageSize)
    }
  }

  return (
    <div className="flex flex-col gap-3 border-t border-border px-4 py-3 sm:flex-row sm:items-center sm:justify-between">
      <label className="flex items-center gap-2 text-xs text-muted-foreground">
        {t('common.pagination.rowsPerPage')}
        <Input
          className="h-8 w-16 px-2 text-center text-xs text-foreground"
          type="number"
          min={1}
          max={100}
          value={pageSizeInput}
          onChange={(event) => setPageSizeInput(event.target.value)}
          onBlur={commitPageSize}
          onKeyDown={(event) => {
            if (event.key === 'Enter') event.currentTarget.blur()
          }}
        />
      </label>

      <div className="flex items-center justify-between gap-3 sm:justify-end">
        <span className="text-xs text-muted-foreground">
          {t('common.pagination.page', { page, totalPages })}
        </span>
        <div className="flex items-center gap-1.5">
          <Button
            variant="outline"
            size="icon"
            className="size-8"
            onClick={() => onPageChange(page - 1)}
            disabled={page <= 1}
            aria-label={t('common.pagination.previous')}
            title={t('common.pagination.previous')}
          >
            <CaretLeftIcon size={14} />
          </Button>
          <Button
            variant="outline"
            size="icon"
            className="size-8"
            onClick={() => onPageChange(page + 1)}
            disabled={page >= totalPages || (meta?.totalPages ?? 0) === 0}
            aria-label={t('common.pagination.next')}
            title={t('common.pagination.next')}
          >
            <CaretRightIcon size={14} />
          </Button>
        </div>
      </div>
    </div>
  )
}

export { ListPagination }
