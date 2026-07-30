import { CircleNotchIcon, TrashIcon, WarningIcon } from '@phosphor-icons/react'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useLocation, useNavigate, useOutletContext, useParams } from 'react-router-dom'

import { toFieldErrorMap } from '../api'
import {
  getCampaign,
  getCampaignOptions,
  updateCampaign,
  uploadCampaignBanner,
} from '../api/campaignsApi'
import { CampaignActionManager } from '../components/campaigns/CampaignActionManager'
import { CampaignEditMetadataForm } from '../components/campaigns/CampaignEditMetadataForm'
import { DeleteCampaignDialog } from '../components/campaigns/DeleteCampaignDialog'
import { mapCampaignOptions } from '../components/campaigns/campaignOptions'
import {
  buildCampaignUpdatePayload,
  mapCampaignDetailToFormValues,
} from '../components/campaigns/campaignPayloads'
import { validateCampaignMetadata } from '../components/campaigns/campaignValidation'
import { Breadcrumb } from '../components/layout/Breadcrumb'
import { PageHeader } from '../components/layout/PageHeader'
import { Button } from '../components/ui/button'
import { PermissionCodes } from '../constants/permissionCodes'

function EditCampaignPage() {
  const { i18n, t } = useTranslation()
  const { campaignId } = useParams()
  const navigate = useNavigate()
  const location = useLocation()
  const { hasPermission } = useOutletContext()

  const [campaign, setCampaign] = useState(null)
  const [rawOptions, setRawOptions] = useState({})
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState('')
  const [retryKey, setRetryKey] = useState(0)

  const [isSubmitting, setIsSubmitting] = useState(false)
  const [formError, setFormError] = useState('')
  const [fieldErrors, setFieldErrors] = useState({})

  const [deleteOpen, setDeleteOpen] = useState(false)

  const canUpdate = hasPermission(PermissionCodes.Campaigns.Update)
  const canDelete = hasPermission(PermissionCodes.Campaigns.Delete)
  const isDraft = campaign?.status === 'DRAFT'

  const options = useMemo(
    () => mapCampaignOptions(rawOptions, t),
    [rawOptions, t],
  )

  const initialFormValues = useMemo(
    () => (campaign ? mapCampaignDetailToFormValues(campaign, options) : {}),
    [campaign, options],
  )

  const selectedCampaignVersion = useMemo(
    () =>
      (options.eventTypeVersions || []).find(
        (version) =>
          version.eventTypeVersionId === campaign?.eventDefinition?.eventTypeVersionId ||
          version.value === campaign?.eventDefinition?.eventTypeVersionId,
      ) ?? null,
    [campaign?.eventDefinition?.eventTypeVersionId, options.eventTypeVersions],
  )

  const loadData = useCallback(async (signal) => {
    setIsLoading(true)
    setErrorMessage('')

    try {
      const [campaignResult, optionsResult] = await Promise.all([
        getCampaign(campaignId, signal),
        getCampaignOptions(signal),
      ])

      if (signal?.aborted) return

      setCampaign(campaignResult || null)
      setRawOptions(optionsResult || {})
    } catch (error) {
      if (signal?.aborted) return

      if (error.code === 'CAMPAIGN_NOT_FOUND') {
        navigate('/campaigns', {
          replace: true,
          state: {
            errorMessage:
              error.message ||
              t('campaigns.errors.notFound', { defaultValue: 'Campaign not found.' }),
          },
        })
        return
      }

      setErrorMessage(
        error.message ||
          t('campaigns.errors.loadDetailFailed', {
            defaultValue: 'Failed to load campaign details.',
          }),
      )
    } finally {
      if (!signal?.aborted) {
        setIsLoading(false)
      }
    }
  }, [campaignId, navigate, t])

  useEffect(() => {
    const controller = new AbortController()
    loadData(controller.signal)
    return () => controller.abort()
  }, [loadData, retryKey])

  const handleRefreshActions = useCallback(async () => {
    try {
      const updated = await getCampaign(campaignId)
      if (updated) {
        setCampaign(updated)
      }
    } catch (err) {
      // ignore silent refresh errors
    }
  }, [campaignId])

  async function handleMetadataSubmit(formValues) {
    setFormError('')
    setFieldErrors({})

    const validation = validateCampaignMetadata(formValues, options, t)
    if (!validation.isValid) {
      setFieldErrors(validation.errors)
      return
    }

    setIsSubmitting(true)

    try {
      let bannerKey = formValues.bannerImageKey || null
      if (formValues.bannerFile) {
        const uploaded = await uploadCampaignBanner(formValues.bannerFile)
        if (!uploaded?.key) {
          throw new Error(
            t('campaigns.form.bannerUploadFailed', {
              defaultValue: 'Failed to upload banner image.',
            }),
          )
        }
        bannerKey = uploaded.key
      }

      const payload = buildCampaignUpdatePayload(
        {
          ...formValues,
          bannerImageKey: bannerKey,
        },
        options,
      )

      await updateCampaign(campaignId, payload)

      navigate(`/campaigns/${campaignId}`, {
        replace: true,
        state: {
          successMessage: t('campaigns.updateSuccess', {
            defaultValue: 'Campaign updated successfully.',
          }),
        },
      })
    } catch (error) {
      if (error.code === 'CAMPAIGN_NOT_DRAFT') {
        setErrorMessage(
          error.message ||
            t('campaigns.errors.notDraft', {
              defaultValue: 'Only draft campaigns can be modified.',
            }),
        )
      } else {
        const mapped = toFieldErrorMap(error.details)
        if (Object.keys(mapped).length > 0) {
          setFieldErrors(mapped)
        }
        setFormError(
          error.message ||
            t('campaigns.errors.updateFailed', {
              defaultValue: 'Failed to update campaign metadata.',
            }),
        )
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  const breadcrumbItems = [
    { label: t('campaigns.breadcrumbList', { defaultValue: 'Campaigns' }), to: '/campaigns' },
    {
      label: campaign?.campaignName || campaignId,
      to: `/campaigns/${campaignId}`,
    },
    { label: t('common.edit', { defaultValue: 'Edit' }) },
  ]

  const actions = (
    <div className="flex items-center gap-2">
      {canDelete && isDraft ? (
        <Button
          variant="destructive"
          size="sm"
          onClick={() => setDeleteOpen(true)}
          className="gap-1.5"
        >
          <TrashIcon size={15} weight="bold" />
          {t('common.delete', { defaultValue: 'Delete campaign' })}
        </Button>
      ) : null}
    </div>
  )

  return (
    <>
      <PageHeader
        breadcrumb={<Breadcrumb items={breadcrumbItems} />}
        title={t('campaigns.editTitle', {
          name: campaign?.campaignName || '',
          defaultValue: `Edit campaign: ${campaign?.campaignName || ''}`,
        })}
        description={t('campaigns.editDescription', {
          defaultValue: 'Modify campaign metadata and manage reward actions.',
        })}
        actions={actions}
      />

      {errorMessage ? (
        <div className="mt-5 flex items-center justify-between gap-3 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          <p>{errorMessage}</p>
          <Button variant="outline" size="sm" onClick={() => setRetryKey((k) => k + 1)}>
            {t('common.retry')}
          </Button>
        </div>
      ) : null}

      {!isLoading && campaign && !isDraft ? (
        <div className="mt-5 flex items-center gap-3 rounded-lg border border-amber-500/20 bg-amber-500/10 px-4 py-3 text-[13px] font-medium text-amber-600 dark:text-amber-400">
          <WarningIcon size={20} className="shrink-0" />
          <p>
            {t('campaigns.errors.notDraft', {
              defaultValue:
                'Only draft campaigns can be modified. This campaign is read-only.',
            })}
          </p>
        </div>
      ) : null}

      {isLoading ? (
        <div className="mt-5 flex h-48 items-center justify-center rounded-lg border border-dashed text-sm text-muted-foreground">
          <span className="inline-flex items-center gap-2">
            <CircleNotchIcon className="animate-spin" size={18} aria-hidden="true" />
            {t('campaigns.loading')}
          </span>
        </div>
      ) : campaign ? (
        <div className="mt-6 grid gap-10">
          {/* Metadata section */}
          <div className="grid gap-4">
            <div>
              <h3 className="text-base font-semibold text-foreground">
                {t('campaigns.form.generalTitle', { defaultValue: 'General information' })}
              </h3>
              <p className="mt-0.5 text-xs text-muted-foreground">
                {t('campaigns.editMetadataHelper', {
                  defaultValue: 'Update campaign schedule, limits, and condition.',
                })}
              </p>
            </div>

            <CampaignEditMetadataForm
              initialValues={initialFormValues}
              options={options}
              canUploadBanner={hasPermission(PermissionCodes.Media.Upload)}
              isSubmitting={isSubmitting || !isDraft || !canUpdate}
              formError={formError}
              fieldErrors={fieldErrors}
              onSubmit={handleMetadataSubmit}
              onReset={() => {
                setFormError('')
                setFieldErrors({})
              }}
              t={t}
            />
          </div>

          {/* Action manager section */}
          <div className="border-t border-border pt-6">
            <CampaignActionManager
              campaignId={campaignId}
              actions={campaign.actions || []}
              isDraft={isDraft}
              canEdit={canUpdate}
              options={options}
              selectedVersion={selectedCampaignVersion}
              eventDefinition={campaign.eventDefinition}
              onActionsChanged={handleRefreshActions}
              language={i18n.resolvedLanguage}
              t={t}
            />
          </div>
        </div>
      ) : null}

      <DeleteCampaignDialog
        open={deleteOpen}
        onOpenChange={setDeleteOpen}
        campaign={campaign}
        onSuccess={() => {
          navigate('/campaigns', {
            replace: true,
            state: {
              successMessage: t('campaigns.deleteSuccess', {
                defaultValue: 'Campaign deleted successfully.',
              }),
            },
          })
        }}
        onNotFound={(msg) => {
          navigate('/campaigns', {
            replace: true,
            state: { errorMessage: msg },
          })
        }}
        onNotDraft={(msg) => {
          setErrorMessage(msg)
          setRetryKey((k) => k + 1)
        }}
        t={t}
      />
    </>
  )
}

export { EditCampaignPage }
export default EditCampaignPage
