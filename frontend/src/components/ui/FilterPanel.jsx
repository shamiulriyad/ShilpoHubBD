export default function FilterPanel({ groups = [], className = '' }) {
  return (
    <aside className={`space-y-6 rounded-xl border border-border bg-surface p-5 ${className}`}>
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-semibold text-heading">Filters</h3>
        <button type="button" className="text-xs font-medium text-link hover:underline">
          Clear all
        </button>
      </div>

      {groups.map((group) => (
        <div key={group.label} className="space-y-2 border-t border-border pt-4 first:border-t-0 first:pt-0">
          <p className="text-xs font-semibold uppercase tracking-wide text-body/60">{group.label}</p>
          <ul className="space-y-1.5">
            {group.options.map((option, index) => (
              <li key={`${group.label}-${option}-${index}`}>
                <label className="flex items-center gap-2 text-sm text-body">
                  <input type="checkbox" className="h-3.5 w-3.5 rounded border-border text-primary" />
                  {option}
                </label>
              </li>
            ))}
          </ul>
        </div>
      ))}
    </aside>
  );
}
