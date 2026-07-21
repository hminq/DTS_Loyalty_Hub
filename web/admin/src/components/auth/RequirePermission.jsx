import { Navigate, useOutletContext } from 'react-router-dom'

function RequirePermission({ permission, permissions, children }) {
  const { hasPermission } = useOutletContext()
  const requiredPermissions = permissions ?? (permission ? [permission] : [])

  if (!requiredPermissions.every(hasPermission)) {
    return <Navigate to="/dashboard" replace />
  }

  return children
}

export { RequirePermission }
