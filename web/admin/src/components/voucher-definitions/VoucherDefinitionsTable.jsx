import { CircleNotchIcon } from '@phosphor-icons/react'

import { Badge } from '../ui/badge'
import { Button } from '../ui/button'
import {
  formatVoucherDateTime,
  formatVoucherNumber,
  getVoucherRecordState,
} from './voucherDefinitionFormatters'

function VoucherDefinitionsTable({
  items,
  isLoading,
  isRefreshing,
  onView,
  language,
  t,
}) {
  return (
    <div className="relative overflow-x-auto">
      {isRefreshing ? (
        <div className="absolute right-4 top-3 z-10 flex items-center gap-1.5 text-xs text-muted-foreground">
          <CircleNotchIcon className="animate-spin" size={14} aria-hidden="true" />
          {t('common.refreshing')}
        </div>
      ) : null}

      <table className="w-full min-w-[800px] border-collapse text-left text-[13px]">
        <thead className="bg-muted/55 text-[11px] uppercase tracking-[0.12em] text-muted-foreground">
          <tr>
            <th className="px-4 py-2.5 font-semibold">{t('voucherDefinitions.columns.voucher')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('voucherDefinitions.columns.reward')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('voucherDefinitions.columns.publishType')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('voucherDefinitions.columns.inventory')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('voucherDefinitions.columns.recordState')}</th>
            <th className="px-4 py-2.5 font-semibold">{t('voucherDefinitions.columns.createdAt')}</th>
            <th className="px-4 py-2.5 text-right font-semibold">{t('voucherDefinitions.columns.actions')}</th>
          </tr>
        </thead>
        <tbody>
          {isLoading ? (
            <tr className="border-t border-border">
              <td className="px-4 py-8 text-center text-muted-foreground" colSpan={7}>
                <span className="inline-flex items-center gap-2">
                  <CircleNotchIcon className="animate-spin" size={16} aria-hidden="true" />
                  {t('voucherDefinitions.loading')}
                </span>
              </td>
            </tr>
          ) : (
            items.map((item) => {
              const state = getVoucherRecordState(item.deletedAt)
              const isDeleted = state === 'DELETED'

              return (
                <tr
                  key={item.voucherDefinitionId}
                  className="border-t border-border transition-colors hover:bg-muted/25"
                >
                  <td className="px-4 py-3 font-medium text-foreground">
                    <p className="font-semibold text-foreground">{item.name}</p>
                    <p className="font-mono text-xs text-muted-foreground">{item.code || '—'}</p>
                  </td>
                  <td className="px-4 py-3 text-foreground font-medium">
                    {t(`voucherDefinitions.types.reward.${item.rewardType}`, { defaultValue: item.rewardType })}
                  </td>
                  <td className="px-4 py-3">
                    <Badge variant="outline" className="text-xs font-normal">
                      {t(`voucherDefinitions.types.publish.${item.publishType}`, { defaultValue: item.publishType })}
                    </Badge>
                  </td>
                  <td className="px-4 py-3 font-mono text-xs text-muted-foreground">
                    <span className="font-semibold text-foreground">
                      {formatVoucherNumber(item.remainingStock, language)}
                    </span>
                    {' / '}
                    {formatVoucherNumber(item.totalStock, language)}
                  </td>
                  <td className="px-4 py-3">
                    {isDeleted ? (
                      <Badge variant="destructive" className="text-xs">
                        {t('voucherDefinitions.recordState.DELETED')}
                      </Badge>
                    ) : (
                      <Badge variant="success" className="text-xs">
                        {t('voucherDefinitions.recordState.ACTIVE')}
                      </Badge>
                    )}
                  </td>
                  <td className="px-4 py-3 text-muted-foreground">
                    {formatVoucherDateTime(item.createdAt, language)}
                  </td>
                  <td className="px-4 py-3 text-right">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => onView(item.voucherDefinitionId)}
                    >
                      {t('voucherDefinitions.actions.view')}
                    </Button>
                  </td>
                </tr>
              )
            })
          )}
        </tbody>
      </table>
    </div>
  )
}

export { VoucherDefinitionsTable }
