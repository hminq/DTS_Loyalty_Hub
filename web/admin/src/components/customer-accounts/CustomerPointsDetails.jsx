import { CircleNotchIcon } from '@phosphor-icons/react'

import { Button } from '../ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '../ui/card'
import { CustomerTierProgress } from './CustomerTierProgress'
import { formatCustomerDateTime, formatCustomerNumber } from './customerAccountFormatters'

function CustomerPointsDetails({ points, allTiers, isLoading, errorMessage, onRetry, language, t }) {
  if (isLoading) {
    return (
      <Card className="mt-5 rounded-xl border-border/80 shadow-none">
        <CardHeader className="p-4">
          <CardTitle className="text-sm">{t('customerAccounts.points.title')}</CardTitle>
        </CardHeader>
        <CardContent className="flex items-center gap-2 p-4 pt-0 text-[13px] text-muted-foreground" aria-busy="true">
          <CircleNotchIcon className="animate-spin" size={16} aria-hidden="true" />
          {t('customerAccounts.points.loading')}
        </CardContent>
      </Card>
    )
  }

  if (errorMessage) {
    return (
      <Card className="mt-5 rounded-xl border-border/80 shadow-none">
        <CardHeader className="p-4">
          <CardTitle className="text-sm">{t('customerAccounts.points.title')}</CardTitle>
        </CardHeader>
        <CardContent className="p-4 pt-0">
          <div className="flex flex-wrap items-center justify-between gap-3 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
            <p>{errorMessage}</p>
            {onRetry ? (
              <Button variant="outline" size="sm" onClick={onRetry}>
                {t('customerAccounts.points.retry')}
              </Button>
            ) : null}
          </div>
        </CardContent>
      </Card>
    )
  }

  if (!points) return null

  return (
    <Card className="mt-5 rounded-xl border-border/80 shadow-none">
      <section>
        <CardHeader className="p-4">
          <CardTitle className="text-sm">{t('customerAccounts.points.tierProgressTitle')}</CardTitle>
        </CardHeader>
        <CardContent className="px-4 pb-4 text-[13px]">
          <CustomerTierProgress
            currentTierPoint={points.currentTierPoint}
            nextTierPoint={points.nextTierPoint}
            currentTier={points.tier}
            allTiers={allTiers}
            language={language}
            t={t}
          />
        </CardContent>
      </section>

      <section className="border-t border-border">
        <CardHeader className="p-4">
          <CardTitle className="text-sm">{t('customerAccounts.points.walletTitle')}</CardTitle>
        </CardHeader>
        <CardContent className="grid gap-4 px-4 pb-4 text-[13px] sm:grid-cols-2 lg:grid-cols-3 lg:grid-cols-6">
          <PointMetricItem
            label={t('customerAccounts.points.activePoint')}
            value={formatCustomerNumber(points.activePoint, language)}
            highlight
          />
          <PointMetricItem
            label={t('customerAccounts.points.lockedPoint')}
            value={formatCustomerNumber(points.lockedPoint, language)}
          />
          <PointMetricItem
            label={t('customerAccounts.points.lifetimePoint')}
            value={formatCustomerNumber(points.lifetimePoint, language)}
          />
          <PointMetricItem
            label={t('customerAccounts.points.spentPoint')}
            value={formatCustomerNumber(points.spentPoint, language)}
          />
          <PointMetricItem
            label={t('customerAccounts.points.expiredPoint')}
            value={formatCustomerNumber(points.expiredPoint, language)}
          />
          <PointMetricItem
            label={t('customerAccounts.points.lastUpdated')}
            value={formatCustomerDateTime(points.updatedAt, language) || t('customerAccounts.points.notInitialized')}
          />
        </CardContent>
      </section>
    </Card>
  )
}

function PointMetricItem({ label, value, highlight = false }) {
  return (
    <div className="min-w-0">
      <p className="text-[11px] font-medium uppercase tracking-[0.12em] text-muted-foreground">{label}</p>
      <p className={`mt-1 break-words ${highlight ? 'font-semibold text-foreground text-sm' : 'font-medium text-foreground'}`}>
        {value}
      </p>
    </div>
  )
}

export { CustomerPointsDetails }
