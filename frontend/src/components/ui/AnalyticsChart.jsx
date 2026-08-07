import ChartPlaceholder from './ChartPlaceholder';

export default function AnalyticsChart({ title, type = 'bar', value, trend, height }) {
  return (
    <div className="rounded-xl border border-border bg-surface p-5">
      <div className="mb-1 flex items-center justify-between">
        <p className="text-sm font-semibold text-heading">{title}</p>
        {value && <p className="text-lg font-semibold text-primary">{value}</p>}
      </div>
      {trend && <p className="mb-3 text-xs font-medium text-success">{trend}</p>}
      <ChartPlaceholder type={type} height={height} />
    </div>
  );
}
