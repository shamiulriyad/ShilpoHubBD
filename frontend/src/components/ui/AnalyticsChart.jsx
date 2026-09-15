import { useId } from 'react';
const format = value => value.toLocaleString();
export default function AnalyticsChart({ title, value, data = [], labelKey = 'label', valueKey = 'value', valueFormatter = format, type = 'bar' }) {
  const id = useId();
  const rows = (Array.isArray(data) ? data : []).filter(item => item?.[valueKey] !== null && item?.[valueKey] !== undefined && item?.[valueKey] !== '' && Number.isFinite(Number(item[valueKey]))).map(item => ({ item, label: String(item[labelKey] ?? 'Unknown'), number: Number(item[valueKey]) }));
  const min = Math.min(0, ...rows.map(row => row.number));
  const max = Math.max(0, ...rows.map(row => row.number));
  const span = max - min || 1;
  const x = index => rows.length === 1 ? 334 : 68 + index * 532 / (rows.length - 1);
  const y = number => 215 - ((number - min) / span) * 180;
  const points = rows.map((row,index) => `${x(index)},${y(row.number)}`).join(' ');
  return <section className="min-w-0 rounded-2xl border border-border bg-surface p-5" aria-labelledby={id}>
    <div className="mb-5 flex flex-wrap items-center justify-between gap-3"><h3 id={id} className="text-sm font-semibold">{title}</h3>{value != null && <p className="text-lg font-semibold text-primary">{value}</p>}</div>
    {!rows.length ? <p className="py-10 text-center text-sm text-muted">No analytics data is available yet.</p> : type === 'line' ? <>
      <svg viewBox="0 0 640 260" className="w-full" role="img" aria-label={`${title}: ${rows.length} data points. Exact values are available in the data table below.`}>
        {[0,1,2,3,4].map(tick => { const n = min + span * tick / 4; return <g key={tick}><line x1="68" x2="600" y1={y(n)} y2={y(n)} stroke="currentColor" className="text-border" /><text x="60" y={y(n)+4} textAnchor="end" fontSize="10" fill="currentColor" className="text-muted">{new Intl.NumberFormat(undefined,{notation:'compact',maximumFractionDigits:1}).format(n)}</text></g>; })}
        <polyline points={points} fill="none" stroke="currentColor" strokeWidth="3" strokeLinejoin="round" className="text-primary" />
        {rows.map((row,index)=><g key={index}><circle cx={x(index)} cy={y(row.number)} r="4" fill="currentColor" className="text-primary"><title>{row.label}: {valueFormatter(row.number,row.item)}</title></circle>{(index % Math.max(1,Math.ceil(rows.length / 5)) === 0 || index === rows.length-1) && <text x={x(index)} y="242" textAnchor="middle" fontSize="10" fill="currentColor" className="text-muted">{row.label}</text>}</g>)}
      </svg>
      <details className="mt-3 text-xs"><summary className="cursor-pointer text-primary">View data table</summary><div className="mt-3 max-h-64 overflow-auto"><table className="w-full text-left"><caption className="sr-only">{title}</caption><thead><tr><th className="py-2">Period</th><th className="py-2 text-right">Value</th></tr></thead><tbody>{rows.map((row,index)=><tr className="border-t border-border" key={index}><td className="py-2">{row.label}</td><td className="text-right">{valueFormatter(row.number,row.item)}</td></tr>)}</tbody></table></div></details>
    </> : <div className="space-y-4">{rows.map((row,index)=><div key={index}><div className="mb-2 flex justify-between gap-3 text-xs"><span className="min-w-0 break-words text-muted">{row.label}</span><span className="shrink-0 font-medium">{valueFormatter(row.number,row.item)}</span></div><div className="relative h-2 overflow-hidden rounded-full bg-background" aria-hidden="true"><div className="absolute h-full rounded-full bg-primary" style={{left:`${(Math.min(0,row.number)-min)/span*100}%`,width:`${Math.abs(row.number)/span*100}%`}} /></div></div>)}</div>}
  </section>;
}
