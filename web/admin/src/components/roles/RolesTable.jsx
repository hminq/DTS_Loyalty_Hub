import { useTranslation } from 'react-i18next'

import { Button } from '../ui/button'

function RolesTable({ roles, isLoading, language, capabilities, onView, onEdit, onDelete }) {
  const { t } = useTranslation()
  const hasActions = capabilities.canView || capabilities.canEdit || capabilities.canDelete

  return (
    <div className="relative overflow-x-auto">
      <table className="w-full border-collapse text-left text-[13px]">
        <thead className="bg-muted/70 text-[11px] uppercase tracking-[0.12em] text-muted-foreground">
          <tr>
            <th className="px-3 py-2.5 font-semibold">{t('roles.columns.role')}</th>
            <th className="px-3 py-2.5 font-semibold">{t('roles.columns.permissionCount')}</th>
            <th className="px-3 py-2.5 font-semibold">{t('roles.columns.createdAt')}</th>
            {hasActions ? <th className="px-3 py-2.5 text-right font-semibold">{t('roles.columns.actions')}</th> : null}
          </tr>
        </thead>
        <tbody>
          {isLoading ? (
            <tr className="border-t border-border">
              <td className="px-3 py-5 text-muted-foreground" colSpan={hasActions ? 4 : 3}>{t('roles.loading')}</td>
            </tr>
          ) : roles.map((role) => (
            <tr key={role.roleId} className="border-t border-border">
              <td className="px-3 py-3 font-medium">{role.name}</td>
              <td className="px-3 py-3 text-muted-foreground">{role.permissionIds?.length ?? 0}</td>
              <td className="px-3 py-3 text-muted-foreground">{formatDateTime(role.createdAt, language)}</td>
              {hasActions ? (
                <td className="px-3 py-2 text-right">
                  <div className="flex flex-wrap justify-end gap-1.5">
                    {capabilities.canView ? <Button variant="ghost" size="sm" onClick={() => onView(role.roleId)}>{t('roles.actions.view')}</Button> : null}
                    {capabilities.canEdit ? <Button variant="outline" size="sm" onClick={() => onEdit(role.roleId)}>{t('roles.actions.edit')}</Button> : null}
                    {capabilities.canDelete ? <Button variant="destructive" size="sm" onClick={() => onDelete(role)}>{t('roles.actions.delete')}</Button> : null}
                  </div>
                </td>
              ) : null}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function formatDateTime(value, language) {
  if (!value) return '-'

  return new Intl.DateTimeFormat(language || 'en', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}

export { RolesTable, formatDateTime }
