import { useCallback, useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'

export function useCustomerPagedResource(customerId, fetchFn, defaultErrorMessage) {
  const [searchParams, setSearchParams] = useSearchParams()

  const rawPage = parseInt(searchParams.get('page') || '1', 10)
  const page = Number.isNaN(rawPage) || rawPage < 1 ? 1 : rawPage

  const rawPageSize = parseInt(searchParams.get('pageSize') || '20', 10)
  const pageSize = Number.isNaN(rawPageSize) || rawPageSize < 1 || rawPageSize > 100 ? 20 : rawPageSize

  const [data, setData] = useState([])
  const [meta, setMeta] = useState(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isRefreshing, setIsRefreshing] = useState(false)
  const [errorMessage, setErrorMessage] = useState('')
  const [refreshKey, setRefreshKey] = useState(0)

  useEffect(() => {
    const controller = new AbortController()

    setIsLoading(true)
    setErrorMessage('')

    fetchFn(customerId, { page, pageSize }, controller.signal)
      .then((result) => {
        if (controller.signal.aborted) return
        setData(result?.data ?? [])
        setMeta(result?.meta ?? null)

        if (result?.meta?.totalPages > 0 && page > result.meta.totalPages) {
          setSearchParams((params) => {
            const next = new URLSearchParams(params)
            next.set('page', String(result.meta.totalPages))
            return next
          }, { replace: true })
        }
      })
      .catch((error) => {
        if (controller.signal.aborted) return
        setErrorMessage(error.message || defaultErrorMessage)
      })
      .finally(() => {
        if (!controller.signal.aborted) {
          setIsLoading(false)
          setIsRefreshing(false)
        }
      })

    return () => controller.abort()
  }, [customerId, fetchFn, page, pageSize, defaultErrorMessage, refreshKey, setSearchParams])

  const handlePageChange = useCallback((newPage) => {
    setSearchParams((params) => {
      const next = new URLSearchParams(params)
      next.set('page', String(newPage))
      return next
    })
  }, [setSearchParams])

  const handlePageSizeChange = useCallback((newPageSize) => {
    setSearchParams((params) => {
      const next = new URLSearchParams(params)
      next.set('page', '1')
      next.set('pageSize', String(newPageSize))
      return next
    })
  }, [setSearchParams])

  const retry = useCallback(() => {
    setRefreshKey((k) => k + 1)
  }, [])

  return {
    page,
    pageSize,
    data,
    meta,
    isLoading,
    isRefreshing,
    errorMessage,
    handlePageChange,
    handlePageSizeChange,
    retry,
  }
}
