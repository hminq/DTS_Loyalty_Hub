import {
  ArrowUpRightIcon,
  CreditCardIcon,
  TicketIcon,
  UsersThreeIcon,
} from '@phosphor-icons/react'
import { useTranslation } from 'react-i18next'

import { PageHeader } from '../components/layout/PageHeader'
import { Badge } from '../components/ui/badge'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card'

const overviewCards = [
  { label: 'Active voucher definitions', value: '42', delta: '+12.5%', icon: TicketIcon },
  { label: 'Registered customers', value: '8,420', delta: '+8.2%', icon: UsersThreeIcon },
  { label: 'Issued vouchers', value: '21.6k', delta: '+18.9%', icon: CreditCardIcon },
]

const activityRows = [
  { name: 'Welcome voucher created', type: 'Voucher definition', status: 'Draft', time: '10 min ago' },
  { name: 'Summer campaign published', type: 'Campaign', status: 'Active', time: '34 min ago' },
  { name: 'Admin session revoked', type: 'Security', status: 'Completed', time: '1 hour ago' },
  { name: 'Gold tier threshold updated', type: 'Tier config', status: 'Completed', time: '2 hours ago' },
]

const healthRows = [
  { label: 'Public definitions', value: '18', width: '72%' },
  { label: 'Private pools', value: '9', width: '46%' },
  { label: 'Low stock alerts', value: '4', width: '18%' },
]

function DashboardPage() {
  const { t } = useTranslation()

  return (
    <>
      <PageHeader
        eyebrow={t('dashboard.eyebrow')}
        title={t('dashboard.title')}
        description={t('dashboard.description')}
      />

      <div className="mt-6 grid gap-4 xl:grid-cols-3">
        {overviewCards.map((card) => {
          const Icon = card.icon

          return (
            <Card key={card.label} className="rounded-xl border-border/80 shadow-none">
              <CardHeader className="p-4">
                <div className="flex items-center justify-between">
                  <div className="grid size-10 place-items-center rounded-xl bg-muted text-primary">
                    <Icon size={21} weight="duotone" />
                  </div>
                  <Badge variant="success">
                    <ArrowUpRightIcon size={13} weight="bold" />
                    {card.delta}
                  </Badge>
                </div>
              </CardHeader>
              <CardContent className="p-4 pt-0">
                <p className="text-[13px] font-medium text-muted-foreground">{card.label}</p>
                <p className="mt-2 text-3xl font-semibold tracking-tight">{card.value}</p>
                <p className="mt-2 text-xs text-muted-foreground">{t('dashboard.metrics.period')}</p>
              </CardContent>
            </Card>
          )
        })}
      </div>

      <div className="mt-4 grid gap-4 xl:grid-cols-[1.25fr_0.75fr]">
        <Card className="rounded-xl border-border/80 shadow-none">
          <CardHeader className="p-4">
            <CardTitle className="text-sm">{t('dashboard.activity.title')}</CardTitle>
            <CardDescription className="text-xs">{t('dashboard.activity.description')}</CardDescription>
          </CardHeader>
          <CardContent className="p-4 pt-0">
            <div className="overflow-hidden rounded-xl border border-border">
              <table className="w-full border-collapse text-left text-[13px]">
                <thead className="bg-muted/70 text-[11px] uppercase tracking-[0.12em] text-muted-foreground">
                  <tr>
                    <th className="px-3 py-2.5 font-semibold">{t('dashboard.activity.event')}</th>
                    <th className="px-3 py-2.5 font-semibold">{t('dashboard.activity.type')}</th>
                    <th className="px-3 py-2.5 font-semibold">{t('dashboard.activity.status')}</th>
                    <th className="px-3 py-2.5 font-semibold">{t('dashboard.activity.time')}</th>
                  </tr>
                </thead>
                <tbody>
                  {activityRows.map((row) => (
                    <tr key={`${row.name}-${row.time}`} className="border-t border-border">
                      <td className="px-3 py-3 font-medium">{row.name}</td>
                      <td className="px-3 py-3 text-muted-foreground">{row.type}</td>
                      <td className="px-3 py-3">
                        <Badge variant={row.status === 'Active' ? 'success' : 'secondary'}>
                          {row.status}
                        </Badge>
                      </td>
                      <td className="px-3 py-3 text-muted-foreground">{row.time}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>

        <Card className="rounded-xl border-border/80 shadow-none">
          <CardHeader className="p-4">
            <CardTitle className="text-sm">{t('dashboard.health.title')}</CardTitle>
            <CardDescription className="text-xs">
              {t('dashboard.health.description')}
            </CardDescription>
          </CardHeader>
          <CardContent className="flex flex-col gap-4 p-4 pt-0">
            {healthRows.map((row) => (
              <div key={row.label}>
                <div className="flex items-center justify-between text-[13px]">
                  <span className="font-medium">{row.label}</span>
                  <span className="text-muted-foreground">{row.value}</span>
                </div>
                <div className="mt-2 h-1.5 rounded-full bg-muted">
                  <div className="h-1.5 rounded-full bg-primary" style={{ width: row.width }} />
                </div>
              </div>
            ))}
          </CardContent>
        </Card>
      </div>
    </>
  )
}

export { DashboardPage }
