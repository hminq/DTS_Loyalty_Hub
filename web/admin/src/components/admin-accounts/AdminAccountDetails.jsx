import { PermissionMatrix } from '../permissions/PermissionMatrix'
import { Badge } from '../ui/badge'
import { Card, CardContent, CardHeader, CardTitle } from '../ui/card'
import { formatDateTime } from './AdminAccountsTable'

function AdminAccountDetails({
  account,
  language,
  permissionGroups,
  isPermissionsLoading,
  permissionErrorMessage,
  t,
}) {
  const selectedPermissionIds = (account.role?.permissions ?? [])
    .map((permission) => permission.permissionId)

  return (
    <Card className="mt-5 rounded-xl border-border/80 shadow-none">
      <section>
        <CardHeader><CardTitle>{t('adminAccounts.detail.identity')}</CardTitle></CardHeader>
        <CardContent className="grid gap-4 text-[13px] sm:grid-cols-2 lg:grid-cols-4">
          <DetailItem label={t('adminAccounts.detail.username')} value={account.username} />
          <DetailItem label={t('adminAccounts.detail.email')} value={account.email} />
          <DetailItem label={t('adminAccounts.detail.fullName')} value={account.fullName || t('adminAccounts.detail.emptyValue')} />
          <DetailItem label={t('adminAccounts.detail.phoneNumber')} value={account.phoneNumber || t('adminAccounts.detail.emptyValue')} />
        </CardContent>
      </section>

      <section className="border-t border-border">
        <CardHeader><CardTitle>{t('adminAccounts.detail.access')}</CardTitle></CardHeader>
        <CardContent className="grid gap-4 text-[13px] sm:grid-cols-3">
          <DetailItem label={t('adminAccounts.detail.role')} value={account.roleName} />
          <div>
            <p className="text-[11px] font-medium uppercase tracking-[0.12em] text-muted-foreground">
              {t('adminAccounts.detail.status')}
            </p>
            <div className="mt-1">
              <Badge variant={account.status === 'ENABLE' ? 'success' : 'secondary'}>
                {account.status === 'ENABLE'
                  ? t('adminAccounts.filters.enabled')
                  : t('adminAccounts.filters.disabled')}
              </Badge>
            </div>
          </div>
          <DetailItem
            label={t('adminAccounts.detail.createdAt')}
            value={formatDateTime(account.createdAt, language)}
          />
        </CardContent>
      </section>

      <section className="border-t border-border">
        <CardHeader><CardTitle>{t('adminAccounts.detail.assignedPermissions')}</CardTitle></CardHeader>
        <CardContent>
          {isPermissionsLoading ? (
            <p className="text-[13px] text-muted-foreground">{t('permissions.loading')}</p>
          ) : permissionErrorMessage ? (
            <p className="text-[13px] font-medium text-destructive">{permissionErrorMessage}</p>
          ) : permissionGroups.length === 0 ? (
            <p className="text-[13px] text-muted-foreground">{t('permissions.empty')}</p>
          ) : (
            <PermissionMatrix
              groups={permissionGroups}
              selectedPermissionIds={selectedPermissionIds}
              readOnly
              labels={{
                group: t('permissions.matrix.group'),
                assigned: t('permissions.matrix.assigned'),
                notAssigned: t('permissions.matrix.notAssigned'),
                defined: t('permissions.matrix.defined'),
                notDefined: t('permissions.matrix.notDefined'),
              }}
            />
          )}
        </CardContent>
      </section>
    </Card>
  )
}

function DetailItem({ label, value }) {
  return (
    <div>
      <p className="text-[11px] font-medium uppercase tracking-[0.12em] text-muted-foreground">{label}</p>
      <p className="mt-1 break-words font-medium">{value}</p>
    </div>
  )
}

export { AdminAccountDetails }
