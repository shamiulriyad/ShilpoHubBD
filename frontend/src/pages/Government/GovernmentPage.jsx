import { useState } from 'react';
import { PageHeader, Button } from '../../components/ui';
import { DashboardCard } from '../../components/cards';
import { useNationalOverview, useDistrictRankings, useDashboardSnapshots, useNationalDashboardMutations } from '../../hooks/useNationalDashboard';
import { useHeritageIndexRecords, useComputeHeritageIndex } from '../../hooks/useHeritageIntelligence';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const rankingMetrics = ['sales', 'producers', 'products', 'villages', 'orders'];
const indexTypes = ['HeritageRiskIndex', 'LivingHeritageIndex', 'CraftHealthScore', 'VillageSurvivalIndex', 'YouthParticipation', 'ClimateRiskAnalysis'];

export default function GovernmentPage() {
  const [metric, setMetric] = useState('sales');
  const overviewQuery = useNationalOverview({});
  const rankingsQuery = useDistrictRankings({ metric, top: 10 });
  const snapshotsQuery = useDashboardSnapshots({ pageSize: 10 });
  const { captureSnapshot } = useNationalDashboardMutations();
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ label: '', period: 'Monthly', periodStart: '', periodEnd: '', notes: '' });
  const heritageIndexQuery = useHeritageIndexRecords({ pageSize: 10 });
  const computeIndex = useComputeHeritageIndex();
  const [indexType, setIndexType] = useState('HeritageRiskIndex');

  const overview = overviewQuery.data;

  const handleCapture = (event) => {
    event.preventDefault();
    captureSnapshot.mutate(
      { ...form, periodStart: new Date(form.periodStart).toISOString(), periodEnd: new Date(form.periodEnd).toISOString() },
      { onSuccess: () => { setShowForm(false); setForm({ label: '', period: 'Monthly', periodStart: '', periodEnd: '', notes: '' }); } },
    );
  };

  return (
    <div>
      <PageHeader
        title="National Heritage Dashboard"
        description="Live national heritage-economy metrics, district rankings and historical snapshots."
        action={<Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'Capture Snapshot'}</Button>}
      />

      {overview && (
        <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
          <DashboardCard title="Producers" description="Active / total">
            <p className="text-2xl font-semibold text-primary">{overview.producers.active}/{overview.producers.total}</p>
          </DashboardCard>
          <DashboardCard title="Export Sales" description="This window">
            <p className="text-2xl font-semibold text-primary">৳ {overview.exportGrowth.exportSalesValue.toLocaleString()}</p>
          </DashboardCard>
          <DashboardCard title="Tourism Revenue" description="This window">
            <p className="text-2xl font-semibold text-primary">৳ {overview.tourism.tourismRevenue.toLocaleString()}</p>
          </DashboardCard>
          <DashboardCard title="Districts Covered" description={`of ${overview.coverage.totalDistricts}`}>
            <p className="text-2xl font-semibold text-primary">{overview.coverage.districtsWithProducers}</p>
          </DashboardCard>
          <DashboardCard title="Heritage Economy" description="Total value">
            <p className="text-2xl font-semibold text-primary">৳ {overview.heritageEconomy.totalValue.toLocaleString()}</p>
          </DashboardCard>
          <DashboardCard title="Jobs" description="Filled / posted">
            <p className="text-2xl font-semibold text-primary">{overview.employment.jobsFilled}/{overview.employment.jobsPosted}</p>
          </DashboardCard>
          <DashboardCard title="Tourism Bookings" description="Completed">
            <p className="text-2xl font-semibold text-primary">{overview.tourism.completedBookings}</p>
          </DashboardCard>
          <DashboardCard title="Villages" description="Represented">
            <p className="text-2xl font-semibold text-primary">{overview.coverage.villages}</p>
          </DashboardCard>
        </div>
      )}

      {showForm && (
        <form onSubmit={handleCapture} className="mt-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <input required placeholder="Label" value={form.label} onChange={(e) => setForm((p) => ({ ...p, label: e.target.value }))} className={inputClass} />
          <select value={form.period} onChange={(e) => setForm((p) => ({ ...p, period: e.target.value }))} className={inputClass}>
            {['Monthly', 'Quarterly', 'Yearly', 'Custom'].map((p) => <option key={p} value={p}>{p}</option>)}
          </select>
          <input required type="date" value={form.periodStart} onChange={(e) => setForm((p) => ({ ...p, periodStart: e.target.value }))} className={inputClass} />
          <input required type="date" value={form.periodEnd} onChange={(e) => setForm((p) => ({ ...p, periodEnd: e.target.value }))} className={inputClass} />
          <textarea rows={2} placeholder="Notes" value={form.notes} onChange={(e) => setForm((p) => ({ ...p, notes: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={captureSnapshot.isPending}>
            {captureSnapshot.isPending ? 'Capturing…' : 'Capture Snapshot'}
          </Button>
        </form>
      )}

      <div className="mt-8 grid gap-6 lg:grid-cols-2">
        <div>
          <div className="mb-2 flex items-center justify-between">
            <h3 className="text-sm font-semibold text-heading">District Rankings</h3>
            <select value={metric} onChange={(e) => setMetric(e.target.value)} className={inputClass}>
              {rankingMetrics.map((m) => <option key={m} value={m}>{m}</option>)}
            </select>
          </div>
          <div className="space-y-1">
            {(rankingsQuery.data || []).map((r) => (
              <div key={r.districtId} className="flex items-center justify-between rounded-lg border border-border bg-surface px-3 py-2 text-sm">
                <span>#{r.rank} {r.name} ({r.division})</span>
                <span className="font-medium text-primary">{r.value.toLocaleString()}</span>
              </div>
            ))}
            {(rankingsQuery.data || []).length === 0 && <p className="text-sm text-body/60">No ranking data yet.</p>}
          </div>
        </div>

        <div>
          <h3 className="mb-2 text-sm font-semibold text-heading">Snapshots</h3>
          <div className="space-y-1">
            {(snapshotsQuery.data?.items || []).map((s) => (
              <div key={s.id} className="rounded-lg border border-border bg-surface px-3 py-2 text-sm">
                <p className="font-medium text-heading">{s.label} ({s.period})</p>
                <p className="text-xs text-body/60">৳ {s.heritageEconomyValue.toLocaleString()} heritage economy · {s.totalProducers} producers</p>
              </div>
            ))}
            {(snapshotsQuery.data?.items || []).length === 0 && <p className="text-sm text-body/60">No snapshots captured yet.</p>}
          </div>
        </div>
      </div>

      <div className="mt-8">
        <div className="mb-2 flex flex-wrap items-center justify-between gap-2">
          <h3 className="text-sm font-semibold text-heading">Heritage Intelligence Index</h3>
          <div className="flex gap-2">
            <select value={indexType} onChange={(e) => setIndexType(e.target.value)} className={inputClass}>
              {indexTypes.map((t) => <option key={t} value={t}>{t}</option>)}
            </select>
            <Button variant="primary" disabled={computeIndex.isPending} onClick={() => computeIndex.mutate({ indexType, scope: 'National' })}>
              {computeIndex.isPending ? 'Computing…' : 'Compute Index'}
            </Button>
          </div>
        </div>
        <div className="space-y-1">
          {(heritageIndexQuery.data?.items || []).map((r) => (
            <div key={r.id} className="flex items-center justify-between rounded-lg border border-border bg-surface px-3 py-2 text-sm">
              <span>{r.indexType} · {r.scopeLabel}</span>
              <span className="font-medium text-primary">{r.score.toFixed(1)} ({r.rating})</span>
            </div>
          ))}
          {(heritageIndexQuery.data?.items || []).length === 0 && <p className="text-sm text-body/60">No heritage index scores computed yet.</p>}
        </div>
      </div>
    </div>
  );
}
