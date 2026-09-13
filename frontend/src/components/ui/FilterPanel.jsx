const optionLabel = (option) => (typeof option === 'object' ? option.label : option);
const optionValue = (option) => (typeof option === 'object' ? option.value : option);

export default function FilterPanel({ groups = [], values = {}, onChange, onClear, className = '' }) {
  const hasSelection = Object.values(values).some((value) => value !== undefined && value !== null && value !== '');

  return (
    <aside className={`space-y-6 rounded-xl border border-border bg-surface p-5 ${className}`} aria-label="Filters">
      <div className="flex items-center justify-between gap-3">
        <h3 className="text-sm font-semibold text-heading">Filters</h3>
        {onClear && (
          <button
            type="button"
            onClick={onClear}
            disabled={!hasSelection}
            className="text-xs font-medium text-link hover:underline disabled:cursor-not-allowed disabled:opacity-40"
          >
            Clear all
          </button>
        )}
      </div>

      {groups.map((group) => {
        const key = group.key || group.label;
        const selected = values[key];

        return (
          <fieldset key={key} className="space-y-2 border-t border-border pt-4 first:border-t-0 first:pt-0">
            <legend className="text-xs font-semibold uppercase tracking-wide text-body/60">{group.label}</legend>
            <ul className="space-y-1.5">
              {group.options.map((option, index) => {
                const label = optionLabel(option);
                const value = optionValue(option);
                const checked = selected === value;

                return (
                  <li key={`${key}-${String(value)}-${index}`}>
                    <label className="flex cursor-pointer items-center gap-2 text-sm text-body">
                      <input
                        type="checkbox"
                        checked={checked}
                        onChange={(event) => onChange?.(key, value, event.target.checked)}
                        disabled={!onChange}
                        className="h-3.5 w-3.5 rounded border-border text-primary disabled:cursor-not-allowed disabled:opacity-50"
                      />
                      <span>{label}</span>
                    </label>
                  </li>
                );
              })}
            </ul>
          </fieldset>
        );
      })}
    </aside>
  );
}