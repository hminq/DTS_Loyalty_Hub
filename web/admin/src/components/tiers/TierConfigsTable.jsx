import { CircleNotchIcon } from '@phosphor-icons/react'

import { Button } from '../ui/button'

function TierConfigsTable({
  tiers,
  isLoading,
  canEdit,
  onView,
  onEdit,
  language,
  t,
}) {
  return (
    <div className="relative overflow-x-auto">
      <table className="w-full min-w-[700px] border-collapse text-left text-[13px]">
        <thead className="bg-muted/55 text-[11px] uppercase tracking-[0.12em] text-muted-foreground">
          <tr>
            <th className="px-4 py-2.5 font-semibold">{t('tiers.columns.name')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('tiers.columns.pointsRequired')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('tiers.columns.cycleMonth')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('tiers.columns.priority')}</th>
            <th className="px-4 py-2.5 text-right font-semibold">{t('tiers.columns.actions')}</th>
          </tr>
        </thead>
        <tbody>
          {isLoading ? (
            <tr className="border-t border-border">
              <td className="px-4 py-8 text-center text-muted-foreground" colSpan={5}>
                <span className="inline-flex items-center gap-2">
                  <CircleNotchIcon className="animate-spin" size={16} aria-hidden="true" />
                  {t('tiers.loading')}
                </span>
              </td>
            </tr>
          ) : tiers.length === 0 ? (
            <tr className="border-t border-border">
              <td className="px-4 py-8 text-center text-muted-foreground" colSpan={5}>
                {t('tiers.empty')}
              </td>
            </tr>
          ) : (
            tiers.map((tier) => (
              <tr key={tier.tierConfigId} className="border-t border-border transition-colors hover:bg-muted/25">
                <td className="px-4 py-3 font-semibold text-foreground">
                  {tier.name}
                </td>
                <td className="px-4 py-3 font-medium text-foreground">
                  {formatNumber(tier.pointsRequired, language)} pt
                </td>
                <td className="px-4 py-3 text-muted-foreground">
                  {tier.cycleMonth} {t('tiers.months')}
                </td>
                <td className="px-4 py-3 font-medium text-foreground">
                  {tier.priority}
                </td>
                <td className="px-4 py-3">
                  <div className="flex items-center justify-end gap-2 whitespace-nowrap">
                    <Button variant="ghost" size="sm" onClick={() => onView(tier.tierConfigId)}>
                      {t('tiers.actions.view')}
                    </Button>
                    {canEdit ? (
                      <Button variant="outline" size="sm" onClick={() => onEdit(tier.tierConfigId)}>
                        {t('tiers.actions.edit')}
                      </Button>
                    ) : null}
                  </div>
                </td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  )
}

function formatNumber(value, language) {
  if (value === null || value === undefined) return '0'
  return new Intl.NumberFormat(language || 'en').format(value)
}

export { TierConfigsTable }
