import { Card, CardContent, CardHeader, CardTitle } from '../ui/card'

function TierConfigDetails({ tier, language, t }) {
  return (
    <Card className="mt-5 rounded-xl border-border/80 shadow-none">
      <CardHeader className="p-4">
        <CardTitle className="text-sm">{t('tiers.detail.sectionTitle')}</CardTitle>
      </CardHeader>
      <CardContent className="grid gap-4 px-4 pb-4 text-[13px] sm:grid-cols-2 lg:grid-cols-3">
        <DetailItem label={t('tiers.detail.name')} value={tier.name} />
        <DetailItem label={t('tiers.detail.pointsRequired')} value={`${formatNumber(tier.pointsRequired, language)} pt`} />
        <DetailItem label={t('tiers.detail.cycleMonth')} value={`${tier.cycleMonth} ${t('tiers.months')}`} />
        <DetailItem label={t('tiers.detail.priority')} value={tier.priority} />
        <DetailItem
          label={t('tiers.detail.createdAt')}
          value={formatDateTime(tier.createdAt, language)}
        />
        <DetailItem label={t('tiers.detail.tierConfigId')} value={tier.tierConfigId} code />
      </CardContent>
    </Card>
  )
}

function DetailItem({ label, value, code = false }) {
  return (
    <div className="min-w-0">
      <p className="text-[11px] font-medium uppercase tracking-[0.12em] text-muted-foreground">{label}</p>
      <p className={code ? 'mt-1 break-all font-mono text-xs' : 'mt-1 break-words font-medium'}>{value}</p>
    </div>
  )
}

function formatNumber(value, language) {
  if (value === null || value === undefined) return '0'
  return new Intl.NumberFormat(language || 'en').format(value)
}

function formatDateTime(value, language) {
  if (!value) return '-'
  return new Intl.DateTimeFormat(language || 'en', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}

export { TierConfigDetails }
