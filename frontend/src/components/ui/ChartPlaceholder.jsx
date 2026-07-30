export default function ChartPlaceholder({ title, type = 'bar', height = 220 }) {
  const bars = [40, 65, 30, 80, 55, 70, 45];

  return (
    <div className="rounded-xl border border-border bg-surface p-5">
      {title && <p className="mb-4 text-sm font-semibold text-heading">{title}</p>}
      <div
        className="flex items-end justify-between gap-2 rounded-lg border border-dashed border-border bg-background/40 px-4"
        style={{ height }}
      >
        {type === 'bar' &&
          bars.map((h, i) => (
            <div key={i} className="w-full rounded-t-md bg-primary/30" style={{ height: `${h}%` }} />
          ))}
        {type === 'line' && (
          <svg viewBox="0 0 100 40" preserveAspectRatio="none" className="h-2/3 w-full text-primary/50">
            <polyline
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              points="0,30 15,20 30,25 45,10 60,18 75,5 100,15"
            />
          </svg>
        )}
        {type === 'donut' && (
          <div className="mx-auto h-28 w-28 rounded-full border-[14px] border-primary/30 border-t-primary/70" />
        )}
      </div>
      <p className="mt-3 text-center text-xs text-body/50">Chart placeholder — data wiring not implemented</p>
    </div>
  );
}
