import { useTranslation } from 'react-i18next'

import { cn } from '../lib/utils'
import logoUrl from '../assets/brand/logo.png'

function BrandMark({ className, compact = false }) {
  const { t } = useTranslation()

  return (
    <div className={cn('flex items-center gap-3', className)}>
      <img className="size-10 shrink-0 rounded-xl object-cover shadow-sm" src={logoUrl} alt="Loyalty Hub" />
      {!compact && (
        <div className="leading-none">
          <p className="text-sm font-semibold tracking-tight">Loyalty Hub</p>
          <p className="mt-1 text-[11px] font-medium uppercase tracking-[0.16em] text-muted-foreground">{t('common.adminPortal')}</p>
        </div>
      )}
    </div>
  )
}

export { BrandMark }
