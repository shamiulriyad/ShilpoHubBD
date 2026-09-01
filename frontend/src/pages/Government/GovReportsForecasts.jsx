import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useGovReports, useGovForecasts, useGovReportMutations } from '../../hooks/useGovReports';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const statusTone = { Draft: 'neutral', Published: 'success', Archived: 'neutral' };

function ReportsTab() {
  const { data, isLoading, isError, error } = useGovReports({ pageSize: 50 });
  const { generateReport, updateReport, removeReport } = useGovReportMutations();
  const [form, setForm] = useState({ title: '', reportType: 'Monthly', highlights: '', publish: false });

  const reports = data?.items || [];

  const handleGenerate = (event) => {
    event.preventDefault();
    generateReport.mutate(form, { onSuccess: () => setForm({ title: '', reportType: 'Monthly', highlights: '', publish: false }) });
  };

  return (
    <div>
      <form onSubmit={handleGenerate} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
        <input required placeholder="Report title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
        <select value={form.reportType} onChange={(e) => setForm((p) => ({ ...p, reportType: e.target.value }))} className={inputClass}>
          {['Monthly', 'Quarterly', 'Annual', 'Custom'].map((t) => <option key={t} value={t}>{t}</option>)}
        </select>
        <label className="flex items-center gap-2 text-sm text-body/70">
          <input type="checkbox" checked={form.publish} onChange={(e) => setForm((p) => ({ ...p, publish: e.target.checked }))} /> Publish immediately
        </label>
        <textarea rows={2} placeholder="Highlights" value={form.highlights} onChange={(e) => setForm((p) => ({ ...p, highlights: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
        <Button type="submit" variant="primary" className="sm:col-span-2" disabled={generateReport.isPending}>
          {generateReport.isPending ? 'Generating…' : 'Generate Report'}
        </Button>
      </form>

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-2">
          {reports.map((r) => (
            <div key={r.id} className="flex flex-wrap items-center justify-between gap-2 rounded-xl border border-border bg-surface p-4">
              <div>
                <p className="text-sm font-semibold text-heading">{r.title}</p>
                <p className="text-xs text-body/60">{r.reportType} · {new Date(r.periodStart).toLocaleDateString()} – {new Date(r.periodEnd).toLocaleDateString()}</p>
              </div>
              <div className="flex items-center gap-2">
                <Badge tone={statusTone[r.status] || 'neutral'}>{r.status}</Badge>
                {r.status === 'Draft' && (
                  <button type="button" onClick={() => updateReport.mutate({ id: r.id, payload: { status: 'Published' } })} className="text-xs text-primary hover:underline">Publish</button>
                )}
                <button type="button" onClick={() => removeReport.mutate(r.id)} className="text-xs text-danger hover:underline">Delete</button>
              </div>
            </div>
          ))}
          {reports.length === 0 && <p className="text-sm text-body/60">No reports generated yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}

function ForecastsTab() {
  const { data, isLoading, isError, error } = useGovForecasts({ pageSize: 50 });
  const { generateForecast, removeForecast } = useGovReportMutations();
  const [form, setForm] = useState({ title: '', horizonMonths: 12 });

  const forecasts = data?.items || [];

  const handleGenerate = (event) => {
    event.preventDefault();
    generateForecast.mutate({ ...form, horizonMonths: Number(form.horizonMonths) || 12 }, { onSuccess: () => setForm({ title: '', horizonMonths: 12 }) });
  };

  return (
    <div>
      <form onSubmit={handleGenerate} className="mb-6 flex flex-wrap gap-2 rounded-xl border border-border bg-surface p-4">
        <input required placeholder="Forecast title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className={`${inputClass} flex-1`} />
        <input type="number" min="1" max="60" value={form.horizonMonths} onChange={(e) => setForm((p) => ({ ...p, horizonMonths: e.target.value }))} className={`${inputClass} w-24`} />
        <Button type="submit" variant="primary" disabled={generateForecast.isPending}>{generateForecast.isPending ? 'Generating…' : 'Generate Forecast'}</Button>
      </form>

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-2">
          {forecasts.map((f) => (
            <div key={f.id} className="flex items-center justify-between rounded-xl border border-border bg-surface p-4">
              <div>
                <p className="text-sm font-semibold text-heading">{f.title}</p>
                <p className="text-xs text-body/60">{f.horizonMonths}-month horizon · baseline {new Date(f.baselineAsOf).toLocaleDateString()}</p>
              </div>
              <button type="button" onClick={() => removeForecast.mutate(f.id)} className="text-xs text-danger hover:underline">Delete</button>
            </div>
          ))}
          {forecasts.length === 0 && <p className="text-sm text-body/60">No forecasts generated yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}

export default function GovReportsForecasts() {
  const [tab, setTab] = useState('reports');

  return (
    <div>
      <PageHeader title="Reports & Forecasts" description="Generate period reports and project national heritage-economy metrics forward." />

      <div className="mb-4 flex gap-2 border-b border-border">
        <button type="button" onClick={() => setTab('reports')} className={`border-b-2 px-3 py-2 text-sm font-medium ${tab === 'reports' ? 'border-primary text-primary' : 'border-transparent text-body/60'}`}>Reports</button>
        <button type="button" onClick={() => setTab('forecasts')} className={`border-b-2 px-3 py-2 text-sm font-medium ${tab === 'forecasts' ? 'border-primary text-primary' : 'border-transparent text-body/60'}`}>Forecasts</button>
      </div>

      {tab === 'reports' ? <ReportsTab /> : <ForecastsTab />}
    </div>
  );
}
