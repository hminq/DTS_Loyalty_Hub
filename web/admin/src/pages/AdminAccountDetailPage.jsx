import { ArrowLeftIcon, CircleNotchIcon } from '@phosphor-icons/react'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useLocation, useNavigate, useParams } from 'react-router-dom'

import { getAdminAccount } from '../api/adminAccountsApi'
import { formatDateTime } from '../components/admin-accounts/AdminAccountsTable'
import { PageHeader } from '../components/layout/PageHeader'
import { Badge } from '../components/ui/badge'
import { Button } from '../components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card'

function AdminAccountDetailPage() {
  const { i18n, t } = useTranslation()
  const navigate = useNavigate()
  const location = useLocation()
  const { adminId } = useParams()
  const [account, setAccount] = useState(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState('')

  useEffect(() => {
    let isCurrent = true

    async function loadAccount() {
      try {
        const result = await getAdminAccount(adminId)
        if (isCurrent) setAccount(result)
      } catch (error) {
        if (isCurrent) setErrorMessage(error.message || t('errors.loadAdminAccount'))
      } finally {
        if (isCurrent) setIsLoading(false)
      }
    }

    loadAccount()
    return () => { isCurrent = false }
  }, [adminId])

  return (
    <>
      <PageHeader
        eyebrow={t('adminAccounts.detail.eyebrow')}
        title={account?.fullName || account?.username || t('adminAccounts.detail.titleFallback')}
        description={account ? `@${account.username}` : undefined}
        actions={(
          <Button variant="outline" size="sm" onClick={() => navigate('/admin-accounts')}>
            <ArrowLeftIcon size={14} />
            {t('adminAccounts.detail.back')}
          </Button>
        )}
      />

      {errorMessage ? (
        <p className="mt-5 rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          {errorMessage}
        </p>
      ) : null}
      {location.state?.successMessage ? (
        <p className="mt-5 rounded-lg border border-success/20 bg-success-muted px-4 py-3 text-[13px] font-medium text-success">
          {location.state.successMessage}
        </p>
      ) : null}

      {isLoading ? (
        <div className="mt-6 flex items-center gap-2 text-[13px] text-muted-foreground">
          <CircleNotchIcon className="animate-spin" size={16} />
          {t('adminAccounts.detail.loading')}
        </div>
      ) : account ? (
        <div className="mt-6 grid gap-4 lg:grid-cols-2">
          <DetailCard
            title={t('adminAccounts.detail.identity')}
            rows={[
              [t('adminAccounts.detail.username'), account.username],
              [t('adminAccounts.detail.email'), account.email],
              [t('adminAccounts.detail.fullName'), account.fullName],
              [t('adminAccounts.detail.phoneNumber'), account.phoneNumber],
            ]}
            emptyValue={t('adminAccounts.detail.emptyValue')}
          />
          <Card className="rounded-xl shadow-none">
            <CardHeader className="border-b border-border p-4">
              <CardTitle className="text-sm">{t('adminAccounts.detail.access')}</CardTitle>
            </CardHeader>
            <CardContent className="grid gap-4 p-4">
              <DetailRow label={t('adminAccounts.detail.role')} value={account.roleName} />
              <div className="flex items-center justify-between gap-4">
                <span className="text-xs text-muted-foreground">{t('adminAccounts.detail.status')}</span>
                <Badge variant={account.status === 'ENABLE' ? 'success' : 'secondary'}>
                  {account.status === 'ENABLE'
                    ? t('adminAccounts.filters.enabled')
                    : t('adminAccounts.filters.disabled')}
                </Badge>
              </div>
              <DetailRow
                label={t('adminAccounts.detail.createdAt')}
                value={formatDateTime(account.createdAt, i18n.resolvedLanguage)}
              />
            </CardContent>
          </Card>
        </div>
      ) : null}
    </>
  )
}

function DetailCard({ title, rows, emptyValue }) {
  return (
    <Card className="rounded-xl shadow-none">
      <CardHeader className="border-b border-border p-4">
        <CardTitle className="text-sm">{title}</CardTitle>
      </CardHeader>
      <CardContent className="grid gap-4 p-4">
        {rows.map(([label, value]) => (
          <DetailRow key={label} label={label} value={value || emptyValue} />
        ))}
      </CardContent>
    </Card>
  )
}

function DetailRow({ label, value }) {
  return (
    <div className="flex items-start justify-between gap-4">
      <span className="text-xs text-muted-foreground">{label}</span>
      <span className="text-right text-[13px] font-medium">{value}</span>
    </div>
  )
}

export { AdminAccountDetailPage }
