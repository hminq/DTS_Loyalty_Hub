import {
  ArrowUpRightIcon,
  CaretDownIcon,
  ChartLineUpIcon,
  CreditCardIcon,
  GearSixIcon,
  GiftIcon,
  HouseIcon,
  IdentificationBadgeIcon,
  TicketIcon,
  UsersThreeIcon,
} from '@phosphor-icons/react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'

import { BrandMark } from '../components/BrandMark'
import { LanguageSwitcher } from '../components/LanguageSwitcher'
import { Badge } from '../components/ui/badge'
import { Button } from '../components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card'
import { logoutAdmin } from '../lib/logout'
import { cn } from '../lib/utils'

const sidebarItems = [
  { id: 'lorem-1', label: 'Lorem ipsum 1', icon: HouseIcon },
  {
    id: 'lorem-2',
    label: 'Lorem ipsum 2',
    icon: TicketIcon,
    children: [
      { id: 'lorem-2-1', label: 'Lorem ipsum 2.1' },
      { id: 'lorem-2-2', label: 'Lorem ipsum 2.2', badge: '3' },
      { id: 'lorem-2-3', label: 'Lorem ipsum 2.3' },
      { id: 'lorem-2-4', label: 'Lorem ipsum 2.4' },
    ],
  },
  { id: 'lorem-3', label: 'Lorem ipsum 3', icon: UsersThreeIcon },
  { id: 'lorem-4', label: 'Lorem ipsum 4', icon: GiftIcon },
  { id: 'lorem-5', label: 'Lorem ipsum 5', icon: IdentificationBadgeIcon },
  { id: 'lorem-6', label: 'Lorem ipsum 6', icon: ChartLineUpIcon },
  { id: 'lorem-7', label: 'Lorem ipsum 7', icon: GearSixIcon },
]

const overviewCards = [
  {
    label: 'Lorem ipsum metric',
    value: '42',
    delta: '+12.5%',
    tone: 'success',
    icon: TicketIcon,
  },
  {
    label: 'Lorem ipsum users',
    value: '8,420',
    delta: '+8.2%',
    tone: 'success',
    icon: UsersThreeIcon,
  },
  {
    label: 'Lorem ipsum issued',
    value: '21.6k',
    delta: '+18.9%',
    tone: 'success',
    icon: CreditCardIcon,
  },
]

const activityRows = [
  { name: 'Lorem ipsum dolor sit amet', type: 'Lorem ipsum', status: 'Draft', time: '10 min ago' },
  { name: 'Consectetur adipiscing elit', type: 'Lorem ipsum', status: 'Active', time: '34 min ago' },
  { name: 'Sed do eiusmod tempor', type: 'Lorem ipsum', status: 'Completed', time: '1 hour ago' },
  { name: 'Incididunt ut labore', type: 'Lorem ipsum', status: 'Completed', time: '2 hours ago' },
]

const healthRows = [
  { label: 'Lorem ipsum 1', value: '18', width: '72%' },
  { label: 'Lorem ipsum 2', value: '9', width: '46%' },
  { label: 'Lorem ipsum 3', value: '4', width: '18%' },
]

function DashboardPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const [logoutError, setLogoutError] = useState('')
  const [isLoggingOut, setIsLoggingOut] = useState(false)
  const [selectedItemId, setSelectedItemId] = useState('lorem-1')
  const [openCategoryIds, setOpenCategoryIds] = useState(() => new Set(['lorem-2']))

  async function handleLogout() {
    setIsLoggingOut(true)
    setLogoutError('')

    const result = await logoutAdmin()

    setIsLoggingOut(false)

    if (result.ok) {
      navigate('/login', { replace: true })
      return
    }

    setLogoutError(t('dashboard.logoutError'))
  }

  function toggleSidebarItem(item) {
    if (!item.children) {
      setSelectedItemId(item.id)
      return
    }

    setSelectedItemId(item.children[0].id)
    setOpenCategoryIds((current) => {
      const next = new Set(current)

      if (next.has(item.id)) {
        next.delete(item.id)
      } else {
        next.add(item.id)
      }

      return next
    })
  }

  function selectSidebarChild(parentId, childId) {
    setSelectedItemId(childId)
    setOpenCategoryIds((current) => {
      const next = new Set(current)
      next.add(parentId)
      return next
    })
  }

  return (
    <main className="min-h-screen bg-background text-foreground">
      <LanguageSwitcher />
      <div className="flex min-h-screen">
        <aside className="hidden w-[260px] shrink-0 flex-col border-r border-sidebar-border bg-sidebar px-4 py-5 text-sidebar-foreground lg:flex">
          <BrandMark />

          <nav className="mt-7 flex-1 space-y-1">
            {sidebarItems.map((item) => {
              const Icon = item.icon
              const isOpen = openCategoryIds.has(item.id)
              const isActiveLeaf = !item.children && selectedItemId === item.id

              return (
                <div key={item.id}>
                  <button
                    type="button"
                    className={cn(
                      'flex h-9 w-full items-center gap-2.5 rounded-lg px-2.5 text-left text-[13px] font-semibold transition-colors',
                      isActiveLeaf && 'bg-sidebar-primary text-sidebar-primary-foreground shadow-sm',
                      !isActiveLeaf && 'text-sidebar-foreground hover:bg-muted',
                    )}
                    onClick={() => toggleSidebarItem(item)}
                    aria-expanded={item.children ? isOpen : undefined}
                  >
                    <Icon size={18} weight={isActiveLeaf ? 'fill' : 'regular'} />
                    <span className="min-w-0 flex-1 truncate">{item.label}</span>
                    {item.children ? (
                      <CaretDownIcon
                        size={14}
                        className={cn('transition-transform', isOpen ? 'rotate-180' : 'rotate-0')}
                      />
                    ) : null}
                  </button>

                  {item.children && isOpen ? (
                    <div className="relative ml-[1.15rem] mt-1 space-y-0.5 py-0.5 pl-5 before:absolute before:left-0 before:top-0 before:h-[calc(100%-0.875rem)] before:w-px before:bg-sidebar-border">
                      {item.children.map((child) => (
                        <button
                          key={child.id}
                          type="button"
                          className={cn(
                            'relative flex h-8 w-full items-center justify-between gap-2 rounded-lg px-2.5 text-left text-[13px] font-semibold transition-colors before:absolute before:-left-5 before:top-1/2 before:h-px before:w-3.5 before:bg-sidebar-border',
                            selectedItemId === child.id
                              ? 'bg-sidebar-primary text-sidebar-primary-foreground shadow-sm'
                              : 'text-muted-foreground hover:bg-muted hover:text-foreground',
                          )}
                          onClick={() => selectSidebarChild(item.id, child.id)}
                        >
                          <span className="min-w-0 flex-1 truncate">{child.label}</span>
                          {child.badge ? (
                            <span className={cn(
                              'grid size-5 shrink-0 place-items-center rounded-full text-[11px]',
                              selectedItemId === child.id ? 'bg-sidebar-primary-foreground/15 text-sidebar-primary-foreground' : 'bg-accent text-primary',
                            )}>
                              {child.badge}
                            </span>
                          ) : null}
                        </button>
                      ))}
                    </div>
                  ) : null}
                </div>
              )
            })}
          </nav>

          <Button className="mt-6 h-8 w-full rounded-lg bg-red-600 text-xs text-white hover:bg-red-700" variant="destructive" size="sm" onClick={handleLogout} disabled={isLoggingOut}>
            {t('dashboard.logout')}
          </Button>
        </aside>

        <section className="min-w-0 flex-1 bg-background px-5 py-6 lg:px-9">
          <header className="flex flex-col gap-4 border-b border-border pb-5 sm:flex-row sm:items-start sm:justify-between">
            <div>
              <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-primary">{t('dashboard.eyebrow')}</p>
              <h1 className="mt-2 text-2xl font-semibold tracking-tight sm:text-3xl">{t('dashboard.title')}</h1>
              <p className="mt-2 max-w-2xl text-[13px] leading-5 text-muted-foreground">{t('dashboard.description')}</p>
            </div>

            <div className="flex items-center gap-3 sm:mr-28">
              <Button className="lg:hidden" variant="outline" size="sm" onClick={handleLogout} disabled={isLoggingOut}>
                {t('dashboard.logout')}
              </Button>
            </div>
          </header>

          {logoutError ? (
            <p className="mt-5 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-sm font-medium text-destructive">{logoutError}</p>
          ) : null}

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
                      <Badge className="px-2 py-0 text-[11px]" variant={card.tone}>
                        <ArrowUpRightIcon size={13} weight="bold" />
                        {card.delta}
                      </Badge>
                    </div>
                  </CardHeader>
                  <CardContent className="p-4 pt-0">
                    <p className="text-[13px] font-medium text-muted-foreground">{card.label}</p>
                    <p className="mt-2 text-3xl font-semibold tracking-tight">{card.value}</p>
                    <p className="mt-2 text-xs text-muted-foreground">Lorem ipsum period</p>
                  </CardContent>
                </Card>
              )
            })}
          </div>

          <div className="mt-4 grid gap-4 xl:grid-cols-[1.25fr_0.75fr]">
            <Card className="rounded-xl border-border/80 shadow-none">
              <CardHeader className="p-4">
                <CardTitle className="text-sm">Lorem ipsum activity</CardTitle>
                <CardDescription className="text-xs">Lorem ipsum dolor sit amet, consectetur adipiscing elit.</CardDescription>
              </CardHeader>
              <CardContent className="p-4 pt-0">
                <div className="overflow-hidden rounded-xl border border-border">
                  <table className="w-full border-collapse text-left text-[13px]">
                    <thead className="bg-muted/70 text-[11px] uppercase tracking-[0.12em] text-muted-foreground">
                      <tr>
                        <th className="px-3 py-2.5 font-semibold">Lorem</th>
                        <th className="px-3 py-2.5 font-semibold">Ipsum</th>
                        <th className="px-3 py-2.5 font-semibold">Status</th>
                        <th className="px-3 py-2.5 font-semibold">Time</th>
                      </tr>
                    </thead>
                    <tbody>
                      {activityRows.map((row) => (
                        <tr key={`${row.name}-${row.time}`} className="border-t border-border">
                          <td className="px-3 py-3 font-medium">{row.name}</td>
                          <td className="px-3 py-3 text-muted-foreground">{row.type}</td>
                          <td className="px-3 py-3">
                            <Badge className="px-2 py-0 text-[11px]" variant={row.status === 'Active' ? 'success' : 'secondary'}>{row.status}</Badge>
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
                <CardTitle className="text-sm">Lorem ipsum health</CardTitle>
                <CardDescription className="text-xs">Lorem ipsum dolor sit amet for the next dashboard iteration.</CardDescription>
              </CardHeader>
              <CardContent className="p-4 pt-0">
                <div className="space-y-4">
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
                </div>
              </CardContent>
            </Card>
          </div>
        </section>
      </div>
    </main>
  )
}

export { DashboardPage }
