import { useCallback, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useOutletContext, useParams } from 'react-router-dom'

import { getCustomerAccountPoints } from '../api/customerAccountsApi'
import { getTierConfigs } from '../api/tiersApi'
import { CustomerDataPageHeader } from '../components/customer-accounts/CustomerDataPageHeader'
import { CustomerPointsDetails } from '../components/customer-accounts/CustomerPointsDetails'
import { PermissionCodes } from '../constants/permissionCodes'

function CustomerPointsPage() {
  const { customerId } = useParams()
  const { i18n, t } = useTranslation()
  const outletContext = useOutletContext()
  const hasPermission = outletContext?.hasPermission || (() => false)

  const [points, setPoints] = useState(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState('')
  const [allTiers, setAllTiers] = useState([])

  const canViewTiers = hasPermission(PermissionCodes.Tiers.View)

  const fetchPoints = useCallback((signal) => {
    setIsLoading(true)
    setErrorMessage('')

    getCustomerAccountPoints(customerId, signal)
      .then((result) => {
        if (!signal?.aborted) setPoints(result)
      })
      .catch((error) => {
        if (signal?.aborted) return
        setErrorMessage(error.message || t('customerAccounts.points.error'))
      })
      .finally(() => {
        if (!signal?.aborted) setIsLoading(false)
      })
  }, [customerId, t])

  useEffect(() => {
    const controller = new AbortController()

    fetchPoints(controller.signal)

    if (canViewTiers) {
      getTierConfigs(controller.signal)
        .then((tiers) => {
          if (!controller.signal.aborted) setAllTiers(tiers ?? [])
        })
        .catch(() => {
          if (!controller.signal.aborted) setAllTiers([])
        })
    } else {
      setAllTiers([])
    }

    return () => controller.abort()
  }, [canViewTiers, customerId, fetchPoints])

  return (
    <CustomerDataPageHeader customerId={customerId} sectionLabel={t('customerAccounts.sections.points')} t={t}>
      <CustomerPointsDetails
        points={points}
        allTiers={allTiers}
        isLoading={isLoading}
        errorMessage={errorMessage}
        onRetry={() => fetchPoints(null)}
        language={i18n.resolvedLanguage}
        t={t}
      />
    </CustomerDataPageHeader>
  )
}

export { CustomerPointsPage }
