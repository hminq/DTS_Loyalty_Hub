import { Badge } from '../ui/badge'
import { Card, CardContent, CardHeader, CardTitle } from '../ui/card'
import { formatCustomerDateTime, formatCustomerNumber } from './customerAccountFormatters'

function CustomerAccountDetails({ account, language, t }) {
  return (
    <Card className="mt-5 rounded-xl border-border/80 shadow-none">
      <section>
        <CardHeader className="p-4">
          <CardTitle className="text-sm">{t('customerAccounts.detail.identity')}</CardTitle>
        </CardHeader>
        <CardContent className="grid gap-4 px-4 pb-4 text-[13px] sm:grid-cols-2 lg:grid-cols-4">
          <DetailItem label={t('customerAccounts.detail.fullName')} value={account.fullName || t('customerAccounts.emptyValue')} />
          <DetailItem label={t('customerAccounts.detail.username')} value={account.username} />
          <DetailItem label={t('customerAccounts.detail.email')} value={account.email} />
          <DetailItem label={t('customerAccounts.detail.phoneNumber')} value={account.phoneNumber || t('customerAccounts.emptyValue')} />
          <StatusItem status={account.status} t={t} />
          <DetailItem
            label={t('customerAccounts.detail.createdAt')}
            value={formatCustomerDateTime(account.createdAt, language) || t('customerAccounts.emptyValue')}
          />
          <DetailItem label={t('customerAccounts.detail.customerId')} value={account.customerId} code />
          <DetailItem label={t('customerAccounts.detail.userId')} value={account.userId} code />
        </CardContent>
      </section>

      <section className="border-t border-border">
        <CardHeader className="p-4">
          <CardTitle className="text-sm">{t('customerAccounts.detail.tier')}</CardTitle>
        </CardHeader>
        <CardContent className="grid gap-4 px-4 pb-4 text-[13px] sm:grid-cols-2 lg:grid-cols-4">
          <DetailItem
            label={t('customerAccounts.detail.tierName')}
            value={account.tier?.name || t('customerAccounts.unassignedTier')}
          />
          <DetailItem
            label={t('customerAccounts.detail.pointsRequired')}
            value={formatCustomerNumber(account.tier?.pointsRequired, language, t('customerAccounts.emptyValue'))}
          />
          <DetailItem
            label={t('customerAccounts.detail.cycleMonth')}
            value={formatCustomerNumber(account.tier?.cycleMonth, language, t('customerAccounts.emptyValue'))}
          />
          <DetailItem
            label={t('customerAccounts.detail.priority')}
            value={formatCustomerNumber(account.tier?.priority, language, t('customerAccounts.emptyValue'))}
          />
        </CardContent>
      </section>
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

function StatusItem({ status, t }) {
  return (
    <div>
      <p className="text-[11px] font-medium uppercase tracking-[0.12em] text-muted-foreground">
        {t('customerAccounts.detail.status')}
      </p>
      <div className="mt-1">
        <Badge variant={status === 'ENABLE' ? 'success' : 'secondary'}>
          {status === 'ENABLE'
            ? t('customerAccounts.status.enabled')
            : t('customerAccounts.status.disabled')}
        </Badge>
      </div>
    </div>
  )
}

export { CustomerAccountDetails }
