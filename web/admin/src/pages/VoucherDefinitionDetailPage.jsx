import { CircleNotchIcon } from '@phosphor-icons/react'
import { useCallback, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useLocation, useNavigate, useParams, useOutletContext } from 'react-router-dom'

import {
  createVoucherPoolImportJob,
  createVoucherPoolImportUploadUrl,
  getVoucherDefinition,
  uploadVoucherPoolCsvToS3,
} from '../api/voucherDefinitionsApi'
import { Breadcrumb } from '../components/layout/Breadcrumb'
import { PageHeader } from '../components/layout/PageHeader'
import { VoucherDefinitionDetails } from '../components/voucher-definitions/VoucherDefinitionDetails'
import { VoucherPoolImportDialog } from '../components/voucher-definitions/VoucherPoolImportDialog'
import { Button } from '../components/ui/button'
import { PermissionCodes } from '../constants/permissionCodes'

function VoucherDefinitionDetailPage() {
  const { voucherDefinitionId } = useParams()
  const { i18n, t } = useTranslation()
  const navigate = useNavigate()
  const location = useLocation()
  const { hasPermission } = useOutletContext()

  const canImportVoucherCodes = hasPermission(PermissionCodes.VoucherDefinitions.Update)

  const [voucher, setVoucher] = useState(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState('')
  const [successMessage, setSuccessMessage] = useState(location.state?.successMessage || '')
  const [refreshKey, setRefreshKey] = useState(0)
  const [importDialogOpen, setImportDialogOpen] = useState(false)

  const returnSearch = location.state?.returnSearch
  const listTarget = returnSearch ? `/voucher-definitions?${returnSearch}` : '/voucher-definitions'

  useEffect(() => {
    if (location.state?.successMessage) {
      window.history.replaceState({}, document.title)
    }
  }, [location.state])

  useEffect(() => {
    const controller = new AbortController()
    setIsLoading(true)
    setErrorMessage('')

    getVoucherDefinition(voucherDefinitionId, controller.signal)
      .then((data) => {
        if (!controller.signal.aborted) setVoucher(data)
      })
      .catch((error) => {
        if (controller.signal.aborted) return

        if (error.code === 'VOUCHER_DEFINITION_NOT_FOUND') {
          navigate(listTarget, {
            replace: true,
            state: { errorMessage: t('voucherDefinitions.errors.notFound') },
          })
          return
        }

        setErrorMessage(error.message || t('voucherDefinitions.errors.loadDetail'))
      })
      .finally(() => {
        if (!controller.signal.aborted) setIsLoading(false)
      })

    return () => controller.abort()
  }, [voucherDefinitionId, refreshKey, navigate, listTarget, t])

  const provisioningStatus = voucher?.poolProvisioning?.status
  useEffect(() => {
    if (provisioningStatus !== 'PENDING' && provisioningStatus !== 'PROCESSING') {
      return undefined
    }

    let timerId
    let cancelled = false
    let controller

    async function poll() {
      if (document.visibilityState === 'hidden') {
        timerId = window.setTimeout(poll, 5000)
        return
      }

      controller = new AbortController()
      try {
        const data = await getVoucherDefinition(voucherDefinitionId, controller.signal)
        if (!cancelled) setVoucher(data)
      } catch (error) {
        if (!cancelled && error.name !== 'CanceledError') {
          setErrorMessage(error.message || t('voucherDefinitions.errors.loadDetail'))
        }
      }

      if (!cancelled) timerId = window.setTimeout(poll, 3000)
    }

    timerId = window.setTimeout(poll, 3000)
    return () => {
      cancelled = true
      window.clearTimeout(timerId)
      controller?.abort()
    }
  }, [provisioningStatus, voucherDefinitionId, t])

  const handleImport = useCallback(async (file) => {
    const upload = await createVoucherPoolImportUploadUrl(voucherDefinitionId, file)
    await uploadVoucherPoolCsvToS3(upload, file)
    await createVoucherPoolImportJob(voucherDefinitionId, upload.objectKey)
    setImportDialogOpen(false)
    setSuccessMessage(t('voucherDefinitions.import.accepted'))
    setRefreshKey((key) => key + 1)
  }, [voucherDefinitionId, t])

  return (
    <>
      <PageHeader
        breadcrumb={<Breadcrumb items={[
          { label: t('voucherDefinitions.title'), to: listTarget },
          { label: voucher?.name || t('voucherDefinitions.detail.titleFallback') },
        ]} />}
        title={
          <div className="flex flex-wrap items-center gap-3">
            <span>{voucher?.name || t('voucherDefinitions.detail.titleFallback')}</span>
            {voucher?.code ? (
              <span className="inline-flex items-center rounded-md border border-primary/30 bg-primary/10 px-2.5 py-1 font-mono text-xs font-bold text-primary tracking-wide">
                {voucher.code}
              </span>
            ) : null}
          </div>
        }
        description={t('voucherDefinitions.detail.description')}
      />

      {successMessage ? (
        <div className="mt-5 rounded-lg border border-success/20 bg-success/10 px-4 py-3 text-[13px] font-medium text-success">
          {successMessage}
        </div>
      ) : null}

      {errorMessage ? (
        <div className="mt-5 flex flex-wrap items-center justify-between gap-3 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          <p>{errorMessage}</p>
          <Button variant="outline" size="sm" onClick={() => setRefreshKey((k) => k + 1)}>
            {t('common.retry')}
          </Button>
        </div>
      ) : null}

      {isLoading ? (
        <div className="mt-5 flex items-center gap-2 text-[13px] text-muted-foreground" aria-busy="true">
          <CircleNotchIcon className="animate-spin" size={16} aria-hidden="true" />
          {t('voucherDefinitions.loading')}
        </div>
      ) : voucher ? (
        <VoucherDefinitionDetails
          voucher={voucher}
          language={i18n.resolvedLanguage}
          canImportVoucherCodes={canImportVoucherCodes}
          onImportVoucherCodes={() => setImportDialogOpen(true)}
          t={t}
        />
      ) : null}

      <VoucherPoolImportDialog
        open={importDialogOpen}
        onClose={() => setImportDialogOpen(false)}
        onImport={handleImport}
      />
    </>
  )
}

export { VoucherDefinitionDetailPage }
