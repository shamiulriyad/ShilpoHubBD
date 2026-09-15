import { useId, useState } from 'react';

const optionLabel = (option) => (typeof option === 'object' ? option.label : option);
const optionValue = (option) => (typeof option === 'object' ? option.value : option);

export default function FilterPanel({ groups = [], values = {}, onChange, onClear, className = '' }) {
  const id = useId();
  const [queries, setQueries] = useState({});
  const selectedGroups = groups.filter((group) => {
    const value = values[group.key || group.label];
    return value !== undefined && value !== null && value !== '';
  });

  return (
    <aside className={`self-start overflow-hidden rounded-2xl border border-border bg-surface shadow-sm ${className}`} aria-label="Product filters">
      <div className="flex items-center justify-between gap-3 border-b border-border px-5 py-4">
        <div>
          <h3 className="text-sm font-semibold text-heading">Refine your search</h3>
          <p className="mt-1 text-xs text-muted">Find a craft that feels like you.</p>
        </div>
        {onClear && (
          <button type="button" onClick={() => { setQueries({}); onClear(); }} disabled={!selectedGroups.length && !Object.values(queries).some(Boolean)} className="shrink-0 text-xs font-semibold text-primary hover:underline disabled:opacity-40">
            Reset
          </button>
        )}
      </div>
      {selectedGroups.length > 0 && (
        <div className="flex flex-wrap gap-2 border-b border-border bg-background/60 px-5 py-3" aria-label="Active filters">
          {selectedGroups.map((group) => {
            const key = group.key || group.label;
            const selected = group.options.find((option) => String(optionValue(option)) === String(values[key]));
            const label = selected ? optionLabel(selected) : group.label;
            return <button key={key} type="button" onClick={() => onChange?.(key, values[key], false)} aria-label={`Remove ${label} filter`} className="rounded-full border border-primary/20 bg-primary/5 px-3 py-1 text-xs font-medium text-primary">{label} <span aria-hidden="true">×</span></button>;
          })}
        </div>
      )}
      <div className="divide-y divide-border px-5">
        {groups.map((group) => {
          const key = group.key || group.label;
          const options = group.options || [];
          const searchable = group.searchable || options.length > 8;
          const query = queries[key] || '';
          const visibleOptions = options.filter((option) => String(optionLabel(option)).toLowerCase().includes(query.toLowerCase()));
          return (
            <fieldset key={key} className="min-w-0 py-4">
              <legend className="float-left mb-3 w-full text-xs font-bold uppercase tracking-wider text-heading">{group.label}</legend>
              <div className="clear-both">
                {searchable && <input type="search" aria-label={`Search ${group.label.toLowerCase()} filters`} placeholder={`Find ${group.label.toLowerCase()}…`} value={query} onChange={(event) => setQueries((previous) => ({ ...previous, [key]: event.target.value }))} className="mb-3 w-full rounded-lg border border-border bg-background/50 px-3 py-2 text-sm outline-none focus:border-primary" />}
                <div className={`workspace-scroll space-y-1 pr-1 ${searchable ? 'max-h-44 overflow-y-auto overscroll-contain' : ''}`}>
                  <label className="flex cursor-pointer items-center gap-3 rounded-lg px-2 py-2 text-sm hover:bg-background">
                    <input type="radio" name={`${id}-${key}`} checked={values[key] === undefined || values[key] === null || values[key] === ''} onChange={() => onChange?.(key, '', false)} disabled={!onChange} className="h-4 w-4 accent-primary" />
                    <span>Any {group.label.toLowerCase()}</span>
                  </label>
                  {visibleOptions.map((option) => {
                    const value = optionValue(option);
                    const checked = String(values[key]) === String(value);
                    return <label key={String(value)} className={`flex cursor-pointer items-center gap-3 rounded-lg px-2 py-2 text-sm transition ${checked ? 'bg-primary/10 font-medium text-primary' : 'text-body hover:bg-background'}`}>
                      <input type="radio" name={`${id}-${key}`} checked={checked} onChange={() => onChange?.(key, value, true)} disabled={!onChange} className="h-4 w-4 shrink-0 accent-primary" />
                      <span>{optionLabel(option)}</span>
                    </label>;
                  })}
                  {visibleOptions.length === 0 && <p className="px-2 py-3 text-xs text-muted">No matching options.</p>}
                </div>
              </div>
            </fieldset>
          );
        })}
      </div>
    </aside>
  );
}
