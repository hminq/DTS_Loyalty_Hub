import { ArrowsLeftRightIcon, CoinsIcon, ReceiptIcon, TicketIcon, UserIcon } from '@phosphor-icons/react'
import { NavLink } from 'react-router-dom'

function CustomerSectionNavigation({ customerId, t }) {
  const navItems = [
    {
      to: `/customer-accounts/${customerId}`,
      end: true,
      label: t('customerAccounts.sections.overview'),
      icon: UserIcon,
    },
    {
      to: `/customer-accounts/${customerId}/points`,
      end: false,
      label: t('customerAccounts.sections.points'),
      icon: CoinsIcon,
    },
    {
      to: `/customer-accounts/${customerId}/vouchers`,
      end: false,
      label: t('customerAccounts.sections.vouchers'),
      icon: TicketIcon,
    },
    {
      to: `/customer-accounts/${customerId}/voucher-redemptions`,
      end: false,
      label: t('customerAccounts.sections.voucherRedemptions'),
      icon: ReceiptIcon,
    },
    {
      to: `/customer-accounts/${customerId}/point-transactions`,
      end: false,
      label: t('customerAccounts.sections.pointTransactions'),
      icon: ArrowsLeftRightIcon,
    },
  ]

  return (
    <nav className="mt-5 flex gap-1 overflow-x-auto border-b border-border pb-2 text-[13px]" aria-label="Customer section navigation">
      {navItems.map((item) => {
        const Icon = item.icon
        return (
          <NavLink
            key={item.to}
            to={item.to}
            end={item.end}
            className={({ isActive }) =>
              `flex shrink-0 items-center gap-2 rounded-lg px-3 py-1.5 transition-colors ${
                isActive
                  ? 'bg-primary font-semibold text-primary-foreground shadow-xs'
                  : 'font-medium text-muted-foreground hover:bg-muted/50 hover:text-foreground'
              }`
            }
          >
            <Icon size={16} aria-hidden="true" />
            <span>{item.label}</span>
          </NavLink>
        )
      })}
    </nav>
  )
}

export { CustomerSectionNavigation }
