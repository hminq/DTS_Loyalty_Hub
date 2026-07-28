import {
  CircleNotchIcon,
  PencilSimpleIcon,
  ProhibitIcon,
  RocketLaunchIcon,
  TrashIcon,
} from '@phosphor-icons/react'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useLocation, useNavigate, useOutletContext, useParams } from 'react-router-dom'

import { getCampaign, getCampaignOptions } from '../api/campaignsApi'
import { ActivateCampaignDialog } from '../components/campaigns/ActivateCampaignDialog'
import { CampaignActionsDetails } from '../components/campaigns/CampaignActionsDetails'
import { CampaignDetails } from '../components/campaigns/CampaignDetails'
import { CampaignSessionsDetails } from '../components/campaigns/CampaignSessionsDetails'
import { CancelCampaignDialog } from '../components/campaigns/CancelCampaignDialog'
import { DeleteCampaignDialog } from '../components/campaigns/DeleteCampaignDialog'
import { mapCampaignOptions } from '../components/campaigns/campaignOptions'
import { Breadcrumb } from '../components/layout/Breadcrumb'
import { PageHeader } from '../components/layout/PageHeader'
import { Button } from '../components/ui/button'
import { PermissionCodes } from '../constants/permissionCodes'

function CampaignDetailPage() {
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
  const [deleteOpen, setDeleteOpen] = useState(false)
  const [activateOpen, setActivateOpen] = useState(false)
  const [cancelOpen, setCancelOpen] = useState(false)

  const [successMessage, setSuccessMessage] = useState(
    location.state?.successMessage || '',
  )

  const canUpdate = hasPermission(PermissionCodes.Campaigns.Update)
  const canDelete = hasPermission(PermissionCodes.Campaigns.Delete)
  const isDraft = campaign?.status === 'DRAFT'
  const isActive = campaign?.status === 'ACTIVE'

  const options = useMemo(
    () => mapCampaignOptions(rawOptions, t),
    [rawOptions, t],
  )

  useEffect(() => {
    if (location.state?.successMessage) {
      window.history.replaceState({}, document.title)
    }
  }, [location.state])

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
            errorMessage: error.message || t('campaigns.errors.notFound', { defaultValue: 'Campaign not found.' }),
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

  const breadcrumbItems = [
    { label: t('campaigns.breadcrumbList', { defaultValue: 'Campaigns' }), to: '/campaigns' },
    { label: campaign?.campaignName || campaignId },
  ]

  const actions = (
    <div className="flex items-center gap-2">
      {canUpdate && isDraft ? (
        <Button
          variant="default"
          size="sm"
          onClick={() => setActivateOpen(true)}
          className="gap-1.5 bg-success text-success-foreground shadow-sm transition-all hover:bg-success/90 hover:shadow"
        >
          <RocketLaunchIcon size={15} weight="bold" />
          {t('campaigns.activate.button', { defaultValue: 'Activate Campaign' })}
        </Button>
      ) : null}

      {canUpdate && isActive ? (
        <Button
          variant="destructive"
          size="sm"
          onClick={() => setCancelOpen(true)}
        >
          <ProhibitIcon data-icon="inline-start" weight="bold" aria-hidden="true" />
          {t('campaigns.cancel.button', {
            defaultValue: 'Cancel campaign',
          })}
        </Button>
      ) : null}

      {canUpdate && isDraft ? (
        <Button
          variant="outline"
          size="sm"
          onClick={() => navigate(`/campaigns/${campaignId}/edit`)}
          className="gap-1.5"
        >
          <PencilSimpleIcon size={15} weight="bold" />
          {t('common.edit')}
        </Button>
      ) : null}

      {canDelete && isDraft ? (
        <Button
          variant="destructive"
          size="sm"
          onClick={() => setDeleteOpen(true)}
          className="gap-1.5"
        >
          <TrashIcon size={15} weight="bold" />
          {t('common.delete')}
        </Button>
      ) : null}
    </div>
  )

  return (
    <>
      <PageHeader
        breadcrumb={<Breadcrumb items={breadcrumbItems} />}
        title={campaign?.campaignName || t('campaigns.detail.title', { defaultValue: 'Campaign detail' })}
        description={campaign?.description || t('campaigns.detail.description', { defaultValue: 'View campaign configuration and reward rules.' })}
        actions={actions}
      />

      {successMessage ? (
        <div className="mt-5 rounded-lg border border-success/20 bg-success-muted px-4 py-3 text-[13px] font-medium text-success">
          {successMessage}
        </div>
      ) : null}

      {errorMessage ? (
        <div className="mt-5 flex items-center justify-between gap-3 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          <p>{errorMessage}</p>
          <Button variant="outline" size="sm" onClick={() => setRetryKey((k) => k + 1)}>
            {t('common.retry')}
          </Button>
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
        <div className="mt-6 grid gap-6">
          <CampaignDetails
            campaign={campaign}
            options={options}
            language={i18n.resolvedLanguage}
            t={t}
          />
          <CampaignActionsDetails
            actions={campaign.actions || []}
            language={i18n.resolvedLanguage}
            t={t}
          />
          <CampaignSessionsDetails
            sessions={campaign.sessions || []}
            sessionCount={campaign.sessionCount}
            language={i18n.resolvedLanguage}
            t={t}
          />
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

      <ActivateCampaignDialog
        open={activateOpen}
        onOpenChange={setActivateOpen}
        campaign={campaign}
        onSuccess={(updatedCampaign) => {
          setCampaign(updatedCampaign)
          setSuccessMessage(
            t('campaigns.activate.success', {
              defaultValue:
                'Campaign activated successfully! Scheduled sessions have been generated.',
            }),
          )
          setRetryKey((k) => k + 1)
        }}
        t={t}
      />

      <CancelCampaignDialog
        open={cancelOpen}
        onOpenChange={setCancelOpen}
        campaign={campaign}
        onSuccess={(result) => {
          setCampaign((currentCampaign) => (
            currentCampaign
              ? {
                  ...currentCampaign,
                  status: result.status,
                  updatedAt: result.cancelledAt,
                }
              : currentCampaign
          ))
          setSuccessMessage(
            t('campaigns.cancel.success', {
              defaultValue:
                'Campaign cancelled successfully. Uncommitted rewards have been stopped.',
            }),
          )
          setRetryKey((key) => key + 1)
        }}
        onNotFound={(message) => {
          navigate('/campaigns', {
            replace: true,
            state: { errorMessage: message },
          })
        }}
        onNotActive={(message) => {
          setErrorMessage(message)
          setRetryKey((key) => key + 1)
        }}
        t={t}
      />
    </>
  )
}

export { CampaignDetailPage }
export default CampaignDetailPage
