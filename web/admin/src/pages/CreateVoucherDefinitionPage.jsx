import { useEffect, useState } from 'react'
import * as React from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useOutletContext } from 'react-router-dom'

import {
  createVoucherDefinition,
  getVoucherDefinitionOptions,
  getVoucherImportTemplate,
  uploadVoucherDefinitionBanner,
} from '../api/voucherDefinitionsApi'
import { PageHeader } from '../components/layout/PageHeader'
import { Button } from '../components/ui/button'
import { VoucherDefinitionForm } from '../components/voucher-definitions/VoucherDefinitionForm'
import { mapVoucherDefinitionOptions } from '../components/voucher-definitions/voucherDefinitionOptions'
import { PermissionCodes } from '../constants/permissionCodes'

export function CreateVoucherDefinitionPage() {
  const { i18n, t } = useTranslation()
  const navigate = useNavigate()
  const { hasPermission } = useOutletContext()
  
  const [rawOptions, setRawOptions] = useState({})
  const [isLoadingOptions, setIsLoadingOptions] = useState(true)
  const [optionsError, setOptionsError] = useState('')
  const [optionsRetryKey, setOptionsRetryKey] = useState(0)

  const options = React.useMemo(
    () => mapVoucherDefinitionOptions(rawOptions, t),
    [rawOptions, t]
  )

  const [isSubmitting, setIsSubmitting] = useState(false)
  const [apiError, setApiError] = useState(null)
  const [uploadedBannerKey, setUploadedBannerKey] = useState(null)
  const [isDownloadingTemplate, setIsDownloadingTemplate] = useState(false)
  const [templateDownloadError, setTemplateDownloadError] = useState('')
  
  const canUploadBanner = hasPermission(PermissionCodes.Media.Upload)

  useEffect(() => {
    const controller = new AbortController()

    async function loadOptions() {
      setIsLoadingOptions(true)
      setOptionsError('')

      try {
        const data = await getVoucherDefinitionOptions(controller.signal)
        if (controller.signal.aborted) return
        setRawOptions(data)
      } catch (error) {
        if (controller.signal.aborted) return
        setOptionsError(error.message || t('voucherDefinitions.errors.loadOptions'))
      } finally {
        if (!controller.signal.aborted) setIsLoadingOptions(false)
      }
    }

    loadOptions()

    return () => controller.abort()
  }, [optionsRetryKey, t])

  const handleSubmit = async (formValues) => {
    setIsSubmitting(true)
    setApiError(null)

    try {
      let bannerKey = uploadedBannerKey

      // Upload banner first if selected and not yet uploaded
      if (formValues.bannerFile && !bannerKey) {
        try {
          const uploadResponse = await uploadVoucherDefinitionBanner(formValues.bannerFile)
          bannerKey = uploadResponse.key
          setUploadedBannerKey(bannerKey)
        } catch (error) {
          const wrappedError = {
            code: 'UPLOAD_FAILED',
            message: error.message || t('voucherDefinitions.errors.submitFailed'),
            details: error.details || [
              { field: 'file', message: error.message || t('voucherDefinitions.errors.submitFailed') }
            ]
          }
          throw wrappedError
        }
      }

      // 2. Submit the create payload
      const payload = {
        code: formValues.code,
        name: formValues.name,
        description: formValues.description,
        bannerImageUrl: bannerKey || null,
        rewardType: formValues.rewardType,
        rewardValue: formValues.rewardValue,
        validityType: formValues.validityType,
        validFrom: formValues.validFrom,
        validTo: formValues.validTo,
        durationDay: formValues.durationDay,
        generationType: formValues.generationType,
        publishType: formValues.publishType,
        totalStock: formValues.totalStock,
      }

      const created = await createVoucherDefinition(payload)

      // 3. Navigate back to detail page on success
      navigate(`/voucher-definitions/${created.voucherDefinitionId}`, {
        replace: true,
        state: { successMessage: t('voucherDefinitions.createSuccess') || 'Voucher definition created successfully.' }
      })
    } catch (error) {
      // Don't re-throw upload errors if they're caught and wrapped above
      const apiErr = error.code === 'UPLOAD_FAILED' ? error : error
      setApiError(apiErr)
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleDownloadTemplate = async () => {
    setIsDownloadingTemplate(true)
    setTemplateDownloadError('')

    try {
      const template = await getVoucherImportTemplate()
      const anchor = document.createElement('a')
      anchor.href = template.downloadUrl
      anchor.download = template.fileName
      document.body.appendChild(anchor)
      anchor.click()
      anchor.remove()
    } catch (error) {
      setTemplateDownloadError(
        error.message || t('voucherDefinitions.errors.downloadImportTemplate'),
      )
    } finally {
      setIsDownloadingTemplate(false)
    }
  }

  return (
    <>
      <PageHeader
        eyebrow={t('voucherDefinitions.eyebrow')}
        title={t('voucherDefinitions.create')}
        description={t('voucherDefinitions.createDescription')}
        backUrl="/voucher-definitions"
      />
      
      <div className="mt-5">
        {isLoadingOptions ? (
          <div className="flex h-32 items-center justify-center rounded-lg border border-dashed text-sm text-muted-foreground">
            {t('voucherDefinitions.loading')}
          </div>
        ) : optionsError ? (
          <div className="flex items-center justify-between gap-3 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
            <p>{optionsError}</p>
            <Button variant="outline" size="sm" onClick={() => setOptionsRetryKey((k) => k + 1)}>
              {t('common.retry')}
            </Button>
          </div>
        ) : (
          <VoucherDefinitionForm
            options={options}
            isSubmitting={isSubmitting}
            apiError={apiError}
            submitLabel={t('common.create')}
            submittingLabel={t('common.creating')}
            onSubmit={handleSubmit}
            onCancel={() => navigate('/voucher-definitions')}
            canUploadBanner={canUploadBanner}
            isDownloadingTemplate={isDownloadingTemplate}
            templateDownloadError={templateDownloadError}
            onDownloadTemplate={handleDownloadTemplate}
            t={t}
          />
        )}
      </div>
    </>
  )
}
