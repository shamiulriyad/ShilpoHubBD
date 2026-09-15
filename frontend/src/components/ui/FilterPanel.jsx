import { useId, useState } from 'react';
const optionLabel = (option) => (typeof option === 'object' ? option.label : option);
const optionValue = (option) => (typeof option === 'object' ? option.value : option);

export default function FilterPanel({ groups = [], values = {}, onChange, onClear, className = '' }) {
  const id = useId();
  const [searches, setSearches] = useState({});
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
            {group.options.length > 8 && <input aria-label={`Search ${group.label}`} placeholder={`Search ${group.label.toLowerCase()}…`} value={searches[key] || ''} onChange={event=>setSearches(previous=>({...previous,[key]:event.target.value}))} className="w-full rounded-lg border border-border bg-background px-3 py-2 text-sm" />}
            <ul className="max-h-56 space-y-1.5 overflow-y-auto overscroll-contain [scrollbar-width:thin]">
              {group.options.filter(option=>String(optionLabel(option)).toLowerCase().includes((searches[key] || '').toLowerCase())).map((option, index) => {
                const label = optionLabel(option);
                const value = optionValue(option);
                const checked = selected === value;

                return (
                  <li key={`${key}-${String(value)}-${index}`}>
                    <label className="flex cursor-pointer items-center gap-2 text-sm text-body">
                      <input
                        type="radio"
                        name={`${id}-${key}`}
                        checked={checked}
                        onChange={(event) => onChange?.(key, value, event.target.checked)}
                        disabled={!onChange}
                        className="h-4 w-4 border-border accent-primary disabled:cursor-not-allowed disabled:opacity-50"
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
