function FilterField({ label, children }) {
  return (
    <label className="grid min-w-0 gap-1.5">
      <span className="text-xs font-medium">{label}</span>
      {children}
    </label>
  )
}

export { FilterField }
