import { Button } from '../ui/button'

function EmptyState({ icon: Icon, title, description, filtered, onClearSearch, t }) {
  return (
    <div className="grid place-items-center px-6 py-16 text-center">
      <div className="grid size-11 place-items-center rounded-full bg-muted text-primary">
        <Icon size={21} />
      </div>
      <h2 className="mt-4 text-sm font-semibold">{title}</h2>
      <p className="mt-1 max-w-sm text-[13px] text-muted-foreground">{description}</p>
      
      {filtered && onClearSearch ? (
        <div className="mt-4 flex gap-2">
          <Button variant="outline" size="sm" onClick={onClearSearch}>
            {t('common.clearFilters')}
          </Button>
        </div>
      ) : null}
    </div>
  )
}

export { EmptyState }
