import { useId } from 'react';
const defaultFormatter = (value) => Number(value).toLocaleString();

export default function AnalyticsChart({ title, value, data = [], labelKey = 'label', valueKey = 'value', valueFormatter = defaultFormatter, type = 'bar' }) {
  const id = useId();
  const points = (Array.isArray(data) ? data : []).filter((item) => item?.[valueKey] !== null && item?.[valueKey] !== undefined && Number.isFinite(Number(item[valueKey]))).map((item) => ({ item, label: String(item[labelKey] ?? 'Unknown'), value: Number(item[valueKey]) }));
  const low = Math.min(0, ...points.map(p => p.value));
  const high = Math.max(0, ...points.map(p => p.value));
  const range = high - low || 1;
  const x = (i) => 64 + (points.length === 1 ? 258 : (i / (points.length - 1)) * 516);
  const y = (n) => 220 - ((n - low) / range) * 180;
  return <section className="min-w-0 rounded-2xl border border-border bg-surface p-5" aria-labelledby={id}>
    <div className="mb-5 flex flex-wrap items-center justify-between gap-3"><h3 id={id} className="text-sm font-semibold text-heading">{title}</h3>{value != null && <p className="text-lg font-semibold text-primary">{value}</p>}</div>
    {points.length === 0 ? <div className="flex min-h-48 items-center justify-center rounded-xl border border-dashed border-border bg-background/50 p-6 text-center text-sm text-muted">No analytics data yet. Your activity will appear here.</div> : type === 'line' ? <>
      <svg viewBox="0 0 620 260" className="w-full overflow-visible" role="img" aria-label={`${title}. ${points.length} data points. Exact values are in the data table below.`}>
        {[0,1,2,3,4].map(tick => { const n = low + range * tick / 4; return <g key={tick}><line x1="64" x2="580" y1={y(n)} y2={y(n)} stroke="currentColor" className="text-border" strokeDasharray="4 4" /><text x="54" y={y(n)+4} textAnchor="end" fontSize="10" fill="currentColor" className="text-muted">{Intl.NumberFormat('en',{notation:'compact',maximumFractionDigits:1}).format(n)}</text></g>; })}
        <polyline points={points.map((p,i)=>`${x(i)},${y(p.value)}`).join(' ')} fill="none" stroke="currentColor" className="text-primary" strokeWidth="3" strokeLinejoin="round" strokeLinecap="round" />
        {points.map((p,i)=><g key={i}><circle cx={x(i)} cy={y(p.value)} r="4" fill="currentColor" className="text-primary"><title>{p.label}: {valueFormatter(p.value,p.item)}</title></circle>{(i===0 || i===points.length-1 || i%Math.ceil(points.length/5)===0) && <text x={x(i)} y="245" textAnchor="middle" fontSize="10" fill="currentColor" className="text-muted">{p.label}</text>}</g>)}
      </svg>
      <details className="mt-3 text-xs text-muted"><summary className="cursor-pointer font-medium">View data table</summary><div className="max-h-64 overflow-auto"><table className="mt-3 w-full text-left"><caption className="sr-only">{title}</caption><thead><tr><th scope="col" className="py-2">Period</th><th scope="col" className="py-2 text-right">Value</th></tr></thead><tbody>{points.map((p,i)=><tr key={i} className="border-t border-border"><td className="py-2">{p.label}</td><td className="text-right">{valueFormatter(p.value,p.item)}</td></tr>)}</tbody></table></div></details>
    </> : <div className="space-y-4">{points.map((p,i)=><div key={i}><div className="mb-2 flex items-start justify-between gap-3 text-xs"><span className="break-words text-muted">{p.label}</span><span className="shrink-0 font-semibold">{valueFormatter(p.value,p.item)}</span></div><div className="relative h-2 rounded-full bg-background" aria-hidden="true"><span className="absolute h-full rounded-full bg-primary" style={{left:`${(Math.min(p.value,0)-low)/range*100}%`,width:`${Math.abs(p.value)/range*100}%`}} /></div></div>)}</div>}
  </section>;
}
