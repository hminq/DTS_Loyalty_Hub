import { useEffect, useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router-dom'

import { getCampaignOptions } from '../api/campaignsApi'
import { CampaignForm } from '../components/campaigns/CampaignForm'
import { mapCampaignOptions } from '../components/campaigns/campaignOptions'
import { PageHeader } from '../components/layout/PageHeader'
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from '../components/ui/breadcrumb'
import { Button } from '../components/ui/button'

function CreateCampaignPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()

  const [rawOptions, setRawOptions] = useState({})
  const [isLoadingOptions, setIsLoadingOptions] = useState(true)
  const [optionsError, setOptionsError] = useState('')
  const [optionsRetryKey, setOptionsRetryKey] = useState(0)

  const options = useMemo(
    () => mapCampaignOptions(rawOptions, t),
    [rawOptions, t],
  )

  useEffect(() => {
    const controller = new AbortController()

    async function loadOptions() {
      setIsLoadingOptions(true)
      setOptionsError('')

      try {
        const response = await getCampaignOptions(controller.signal)
        if (controller.signal.aborted) return
        setRawOptions(response ?? {})
      } catch (error) {
        if (!controller.signal.aborted) {
          setOptionsError(error.message || t('errors.loadCampaignOptions', { defaultValue: 'Failed to load campaign options.' }))
        }
      } finally {
        if (!controller.signal.aborted) {
          setIsLoadingOptions(false)
        }
      }
    }

    loadOptions()
    return () => controller.abort()
  }, [optionsRetryKey, t])

  const breadcrumb = (
    <Breadcrumb>
      <BreadcrumbList>
        <BreadcrumbItem>
          <BreadcrumbLink render={<Link to="/campaigns" />}>
            {t('campaigns.breadcrumbList', { defaultValue: 'Campaigns' })}
          </BreadcrumbLink>
        </BreadcrumbItem>
        <BreadcrumbSeparator />
        <BreadcrumbItem>
          <BreadcrumbPage>
            {t('campaigns.create', { defaultValue: 'New campaign' })}
          </BreadcrumbPage>
        </BreadcrumbItem>
      </BreadcrumbList>
    </Breadcrumb>
  )

  return (
    <>
      <PageHeader
        breadcrumb={breadcrumb}
        title={t('campaigns.create', { defaultValue: 'New campaign' })}
        description={t('campaigns.createDescription', { defaultValue: 'Create a new loyalty campaign and configure rewards.' })}
      />

      <div className="mt-5">
        {isLoadingOptions ? (
          <div className="flex h-32 items-center justify-center rounded-lg border border-dashed text-sm text-muted-foreground">
            {t('campaigns.loadingOptions', { defaultValue: 'Loading campaign options...' })}
          </div>
        ) : optionsError ? (
          <div className="flex items-center justify-between gap-3 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
            <p>{optionsError}</p>
            <Button variant="outline" size="sm" onClick={() => setOptionsRetryKey((k) => k + 1)}>
              {t('common.retry')}
            </Button>
          </div>
        ) : (
          <CampaignForm
            options={options}
            onCancel={() => navigate('/campaigns')}
            t={t}
          />
        )}
      </div>
    </>
  )
}

export { CreateCampaignPage }
