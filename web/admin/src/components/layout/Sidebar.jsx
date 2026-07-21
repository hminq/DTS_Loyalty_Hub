import {
  CaretDownIcon,
  GearSixIcon,
  LifebuoyIcon,
  SignOutIcon,
} from '@phosphor-icons/react'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useLocation, useNavigate } from 'react-router-dom'

import { BrandMark } from '../BrandMark'
import { cn } from '../../lib/utils'

function Sidebar({ admin, navigationItems, isLoggingOut, logoutLabel, onLogout }) {
  const { t } = useTranslation()
  const location = useLocation()
  const navigate = useNavigate()
  const [openCategoryIds, setOpenCategoryIds] = useState(() => new Set(
    navigationItems
      .filter((item) => item.children?.some((child) => isPathActive(location.pathname, child.path)))
      .map((item) => item.id),
  ))

  useEffect(() => {
    const activeCategory = navigationItems.find(
      (item) => item.children?.some((child) => isPathActive(location.pathname, child.path)),
    )

    if (activeCategory) {
      setOpenCategoryIds((current) => new Set(current).add(activeCategory.id))
    }
  }, [location.pathname, navigationItems])

  function toggleCategory(item) {
    const isOpen = openCategoryIds.has(item.id)

    setOpenCategoryIds((current) => {
      const next = new Set(current)

      if (isOpen) {
        next.delete(item.id)
      } else {
        next.add(item.id)
      }

      return next
    })

    if (!isOpen && item.children.length > 0) {
      navigate(item.children[0].path)
    }
  }

  return (
    <aside className="sticky top-0 hidden h-screen w-[260px] shrink-0 flex-col overflow-hidden border-r border-sidebar-border bg-sidebar px-4 py-5 text-sidebar-foreground lg:flex">
      <div className="shrink-0">
        <BrandMark />
      </div>

      <nav className="mt-7 min-h-0 flex-1 overflow-y-auto overscroll-contain pr-1">
        <div className="flex flex-col gap-1">
          {navigationItems.map((item) => {
            const Icon = item.icon
            const isOpen = item.children ? openCategoryIds.has(item.id) : false
            const isActiveLeaf = !item.children && isPathActive(location.pathname, item.path)

            return (
              <div key={item.id}>
                <button
                  type="button"
                  className={cn(
                    'flex h-9 w-full items-center gap-2.5 rounded-lg px-2.5 text-left text-[13px] font-semibold transition-colors',
                    isActiveLeaf
                      ? 'bg-sidebar-primary text-sidebar-primary-foreground shadow-sm'
                      : 'text-sidebar-foreground hover:bg-muted',
                  )}
                  onClick={() => (item.children ? toggleCategory(item) : navigate(item.path))}
                  aria-expanded={item.children ? isOpen : undefined}
                >
                  <Icon size={18} weight={isActiveLeaf ? 'fill' : 'regular'} />
                  <span className="min-w-0 flex-1 truncate">{t(item.labelKey)}</span>
                  {item.children ? (
                    <CaretDownIcon
                      size={14}
                      className={cn('transition-transform', isOpen && 'rotate-180')}
                    />
                  ) : null}
                </button>

                {item.children && isOpen ? (
                  <div className="relative ml-[1.15rem] mt-1 flex flex-col gap-0.5 py-0.5 pl-5 before:absolute before:left-0 before:top-0 before:h-[calc(100%-0.875rem)] before:w-px before:bg-sidebar-border">
                    {item.children.map((child) => {
                      const isActiveChild = isPathActive(location.pathname, child.path)

                      return (
                        <button
                          key={child.id}
                          type="button"
                          className={cn(
                            'relative flex h-8 w-full items-center rounded-lg px-2.5 text-left text-[13px] font-semibold transition-colors before:absolute before:-left-5 before:top-1/2 before:h-px before:w-3.5 before:bg-sidebar-border',
                            isActiveChild
                              ? 'bg-sidebar-primary text-sidebar-primary-foreground shadow-sm'
                              : 'text-muted-foreground hover:bg-muted hover:text-foreground',
                          )}
                          onClick={() => navigate(child.path)}
                        >
                          <span className="min-w-0 flex-1 truncate">{t(child.labelKey)}</span>
                        </button>
                      )
                    })}
                  </div>
                ) : null}
              </div>
            )
          })}
        </div>
      </nav>

      <div className="mt-4 shrink-0 border-t border-sidebar-border pt-3">
        <div className="flex flex-col gap-1">
          <SidebarUtilityItem
            icon={LifebuoyIcon}
            label={t('navigation.support')}
            active={location.pathname === '/support'}
            onClick={() => navigate('/support')}
          />
          <SidebarUtilityItem
            icon={GearSixIcon}
            label={t('navigation.settings')}
            active={location.pathname === '/settings'}
            onClick={() => navigate('/settings')}
          />
          <SidebarUtilityItem
            icon={SignOutIcon}
            label={logoutLabel}
            onClick={onLogout}
            disabled={isLoggingOut}
            destructive
          />
        </div>

        <AccountIdentity admin={admin} />
      </div>
    </aside>
  )
}

function isPathActive(currentPath, itemPath) {
  return currentPath === itemPath || currentPath.startsWith(`${itemPath}/`)
}

function SidebarUtilityItem({ icon: Icon, label, active = false, destructive = false, onClick, disabled }) {
  return (
    <button
      type="button"
      className={cn(
        'flex min-h-9 w-full items-center gap-3 rounded-lg px-2.5 text-[13px] font-medium transition-colors disabled:pointer-events-none disabled:opacity-50',
        active && 'bg-muted text-foreground',
        !active && !destructive && 'text-muted-foreground hover:bg-muted hover:text-foreground',
        destructive && 'text-destructive hover:bg-destructive/10 hover:text-destructive',
      )}
      onClick={onClick}
      disabled={disabled}
    >
      <Icon size={18} className="shrink-0" />
      <span className="min-w-0 flex-1 truncate text-left">{label}</span>
    </button>
  )
}

function AccountIdentity({ admin }) {
  const { t } = useTranslation()
  const displayName = admin?.fullName || admin?.username || t('account.fallbackName')
  const roleName = admin?.roleName || t('account.fallbackRole')

  return (
    <div className="mt-3 border-t border-sidebar-border pt-3">
      <div className="flex items-center gap-3 px-2.5">
        <div className="grid size-9 shrink-0 place-items-center rounded-full bg-sidebar-primary text-xs font-semibold text-sidebar-primary-foreground">
          {getInitials(displayName)}
        </div>
        <div className="min-w-0">
          <p className="truncate text-[13px] font-semibold text-foreground">{displayName}</p>
          <p className="mt-0.5 truncate text-xs text-muted-foreground">{roleName}</p>
        </div>
      </div>
    </div>
  )
}

function getInitials(value) {
  return value
    .trim()
    .split(/\s+/)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join('') || 'A'
}

export { Sidebar }
