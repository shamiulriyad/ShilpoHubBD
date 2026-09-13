const defaultFormatter = (value) => value?.toLocaleString?.() ?? String(value ?? '');

export default function AnalyticsChart({ title, value, data = [], labelKey = 'label', valueKey = 'value', valueFormatter = defaultFormatter }) {
  const numericValues = data.map((item) => Number(item?.[valueKey]) || 0);
  const maxValue = Math.max(...numericValues, 0);

  return (
    <div className="rounded-xl border border-border bg-surface p-5">
      <div className="mb-4 flex items-center justify-between gap-3">
        <p className="text-sm font-semibold text-heading">{title}</p>
        {value !== undefined && value !== null && <p className="text-lg font-semibold text-primary">{value}</p>}
      </div>

      {data.length > 0 ? (
        <div className="space-y-3">
          {data.map((item, index) => {
            const numericValue = numericValues[index];
            const width = maxValue > 0 ? Math.max((numericValue / maxValue) * 100, numericValue > 0 ? 3 : 0) : 0;
            return (
              <div key={`${item?.[labelKey] ?? 'item'}-${index}`}>
                <div className="mb-1 flex items-center justify-between gap-3 text-xs">
                  <span className="truncate text-body/70">{item?.[labelKey] ?? 'Unknown'}</span>
                  <span className="shrink-0 font-medium text-heading">{valueFormatter(numericValue, item)}</span>
                </div>
                <div className="h-2 overflow-hidden rounded-full bg-background" aria-hidden="true">
                  <div className="h-full rounded-full bg-primary transition-[width]" style={{ width: `${width}%` }} />
                </div>
              </div>
            );
          })}
        </div>
      ) : (
        <p className="text-sm text-body/60">No analytics data is available yet.</p>
      )}
    </div>
  );
}