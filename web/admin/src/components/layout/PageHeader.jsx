function PageHeader({ eyebrow, breadcrumb, title, description, actions }) {
  return (
    <header className="border-b border-border pb-5">
      {breadcrumb ?? (
        <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-primary">{eyebrow}</p>
      )}
      <h1 className="mt-2 block text-2xl font-semibold tracking-tight sm:text-3xl">{title}</h1>
      {description ? (
        <p className="mt-2 max-w-2xl text-[13px] leading-5 text-muted-foreground">{description}</p>
      ) : null}
      {actions ? <div className="mt-4 flex flex-wrap items-center gap-2">{actions}</div> : null}
    </header>
  )
}

export { PageHeader }
