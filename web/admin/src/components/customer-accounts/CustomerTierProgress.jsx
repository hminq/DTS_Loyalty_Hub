import { SparkleIcon } from '@phosphor-icons/react'

import { Badge } from '../ui/badge'
import { formatCustomerNumber } from './customerAccountFormatters'

function CustomerTierProgress({ currentTierPoint, nextTierPoint, currentTier, allTiers, language, t }) {
  const hasCurrentTier = Boolean(currentTier && currentTier.tierId)

  const sortedTiers = Array.isArray(allTiers) && allTiers.length > 0
    ? [...allTiers].sort((a, b) => (a.priority ?? 0) - (b.priority ?? 0))
    : []

  if (!hasCurrentTier && sortedTiers.length === 0 && !nextTierPoint) {
    return (
      <div className="rounded-lg border border-border/60 bg-muted/20 px-4 py-6 text-center text-xs text-muted-foreground">
        {t('customerAccounts.points.noTierProgress')}
      </div>
    )
  }

  let activeTier = null
  let nextTier = null
  let isMaxTier = false

  if (sortedTiers.length > 0) {
    const currentTierIndex = sortedTiers.findIndex((t) => t.tierConfigId === currentTier?.tierId)

    if (currentTierIndex >= 0) {
      activeTier = sortedTiers[currentTierIndex]
      if (currentTierIndex < sortedTiers.length - 1) {
        nextTier = sortedTiers[currentTierIndex + 1]
      } else {
        isMaxTier = true
      }
    } else {
      nextTier = sortedTiers.find((t) => (t.pointsRequired ?? 0) >= (currentTierPoint ?? 0)) || sortedTiers[0]
    }
  } else {
    activeTier = currentTier
    if (!nextTierPoint) {
      isMaxTier = hasCurrentTier
    }
  }

  const minPoint = activeTier?.pointsRequired ?? 0
  const maxPoint = nextTier?.pointsRequired ?? (nextTierPoint || minPoint)

  let percentage = 0
  if (isMaxTier) {
    percentage = 100
  } else if (maxPoint > minPoint) {
    const rawRatio = (((currentTierPoint ?? 0) - minPoint) / (maxPoint - minPoint)) * 100
    percentage = Math.min(100, Math.max(0, Math.round(rawRatio)))
  }

  const pointsToNextTier = nextTier
    ? Math.max(0, (nextTier.pointsRequired ?? 0) - (currentTierPoint ?? 0))
    : nextTierPoint
      ? Math.max(0, nextTierPoint - (currentTierPoint ?? 0))
      : 0

  const activeTierName = activeTier?.name || currentTier?.name || t('customerAccounts.unassignedTier')
  const nextTierName = nextTier?.name || (nextTierPoint ? t('customerAccounts.points.nextTierLabel') : null)

  return (
    <div className="rounded-lg border border-border/60 bg-muted/20 p-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          <span className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
            {t('customerAccounts.points.currentTierPoint')}
          </span>
          <span className="text-base font-bold text-foreground">
            {formatCustomerNumber(currentTierPoint ?? 0, language)}
          </span>
        </div>

        <div className="flex items-center gap-2 text-xs">
          {!hasCurrentTier && !nextTierName ? (
            <Badge variant="secondary">
              {t('customerAccounts.unassignedTier')}
            </Badge>
          ) : isMaxTier ? (
            <Badge variant="success" className="gap-1">
              <SparkleIcon size={12} weight="fill" aria-hidden="true" />
              {t('customerAccounts.points.maxTierReached')}
            </Badge>
          ) : nextTierName ? (
            <span className="text-muted-foreground">
              {t('customerAccounts.points.pointsToNextTier', {
                points: formatCustomerNumber(pointsToNextTier, language),
                tier: nextTierName,
              })}
            </span>
          ) : (
            <Badge variant="secondary">
              {t('customerAccounts.unassignedTier')}
            </Badge>
          )}
        </div>
      </div>

      <div className="mt-3">
        <div className="mb-1.5 flex justify-between text-[11px] font-medium text-muted-foreground">
          <span>{activeTierName} ({formatCustomerNumber(minPoint, language)} pt)</span>
          <span>{percentage}%</span>
          <span>
            {isMaxTier
              ? t('customerAccounts.points.maxTier')
              : nextTierName
                ? `${nextTierName} (${formatCustomerNumber(maxPoint, language)} pt)`
                : t('customerAccounts.points.nextTierLabel')}
          </span>
        </div>
        <div className="h-2.5 w-full overflow-hidden rounded-full bg-muted">
          <div
            className="h-full rounded-full bg-primary transition-all duration-500 ease-out"
            style={{ width: `${percentage}%` }}
            role="progressbar"
            aria-valuenow={percentage}
            aria-valuemin={0}
            aria-valuemax={100}
          />
        </div>
      </div>
    </div>
  )
}

export { CustomerTierProgress }
