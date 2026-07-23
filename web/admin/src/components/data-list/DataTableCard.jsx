import { Card, CardContent } from '../ui/card'

function DataTableCard({ children, className = '', ...props }) {
  return (
    <Card className={`rounded-xl border-border/80 shadow-none overflow-hidden ${className}`} {...props}>
      <CardContent className="p-0">{children}</CardContent>
    </Card>
  )
}

export { DataTableCard }
