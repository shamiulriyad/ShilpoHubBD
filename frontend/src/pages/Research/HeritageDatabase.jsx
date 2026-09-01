import { useState } from 'react';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Table, SearchBar, Badge, Button, AsyncState } from '../../components/ui';
import { useDistricts } from '../../hooks/useDistricts';
import { useVillages } from '../../hooks/useVillages';
import { useHeritageDbSummary, useHeritageDatasets, useHeritageRiskRecords, useHeritageDatabaseMutations } from '../../hooks/useHeritageDatabase';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const datasetCategories = ['Producers', 'Products', 'Villages', 'Tourism', 'Crafts', 'Demographics', 'Other'];
const riskLevels = ['Low', 'Medium', 'High', 'Critical'];
const riskLevelTone = { Low: 'success', Medium: 'secondary', High: 'primary', Critical: 'neutral' };

function OverviewTab() {
  const districtsQuery = useDistricts();
  const villagesQuery = useVillages();
  const summaryQuery = useHeritageDbSummary({});

  const villageCountByDistrict = (villagesQuery.data || []).reduce((acc, v) => {
    acc[v.districtId] = (acc[v.districtId] || 0) + 1;
    return acc;
  }, {});

  const rows = (districtsQuery.data || []).map((d) => ({
    district: d.name,
    division: d.division,
    villages: villageCountByDistrict[d.id] || 0,
  }));

  const summary = summaryQuery.data;

  return (
    <div>
      {summary && (
        <div className="mb-6 grid grid-cols-2 gap-3 sm:grid-cols-4">
          <div className="rounded-lg border border-border bg-surface p-3 text-center"><p className="text-lg font-semibold text-primary">{summary.producers}</p><p className="text-xs text-body/60">Producers</p></div>
          <div className="rounded-lg border border-border bg-surface p-3 text-center"><p className="text-lg font-semibold text-primary">{summary.products}</p><p className="text-xs text-body/60">Products</p></div>
          <div className="rounded-lg border border-border bg-surface p-3 text-center"><p className="text-lg font-semibold text-primary">{summary.datasets}</p><p className="text-xs text-body/60">Datasets</p></div>
          <div className="rounded-lg border border-border bg-surface p-3 text-center"><p className="text-lg font-semibold text-primary">{summary.riskRecords}</p><p className="text-xs text-body/60">Risk records</p></div>
        </div>
      )}
      <div className="mb-6 max-w-xl">
        <SearchBar placeholder="Search the heritage database…" />
      </div>
      <AsyncState isLoading={districtsQuery.isLoading} isError={districtsQuery.isError} error={districtsQuery.error}>
        <Table columns={['district', 'division', 'villages']} rows={rows} />
      </AsyncState>
    </div>
  );
}

function DatasetsTab() {
  const { data, isLoading, isError, error } = useHeritageDatasets({ pageSize: 50 });
  const { createDataset, refreshDataset, removeDataset } = useHeritageDatabaseMutations();
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ name: '', description: '', category: 'Producers', isLive: true });

  const datasets = data?.items || [];

  const handleCreate = (event) => {
    event.preventDefault();
    createDataset.mutate(form, { onSuccess: () => { setShowForm(false); setForm({ name: '', description: '', category: 'Producers', isLive: true }); } });
  };

  return (
    <div>
      <div className="mb-4 flex justify-end">
        <Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'New Dataset'}</Button>
      </div>

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <input required placeholder="Dataset name" value={form.name} onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <select value={form.category} onChange={(e) => setForm((p) => ({ ...p, category: e.target.value }))} className={inputClass}>
            {datasetCategories.map((c) => <option key={c} value={c}>{c}</option>)}
          </select>
          <label className="flex items-center gap-2 text-sm text-body/70">
            <input type="checkbox" checked={form.isLive} onChange={(e) => setForm((p) => ({ ...p, isLive: e.target.checked }))} /> Live-updating
          </label>
          <textarea required rows={2} placeholder="Description" value={form.description} onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={createDataset.isPending}>Create Dataset</Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-2">
          {datasets.map((d) => (
            <div key={d.id} className="flex flex-wrap items-center justify-between gap-2 rounded-xl border border-border bg-surface p-4">
              <div>
                <p className="text-sm font-semibold text-heading">{d.name}</p>
                <p className="text-xs text-body/60">{d.category} · {d.recordCount} records · {d.accessLevel}</p>
              </div>
              <div className="flex items-center gap-2">
                <Badge tone={d.status === 'Published' ? 'success' : 'neutral'}>{d.status}</Badge>
                {d.isLive && (
                  <button type="button" onClick={() => refreshDataset.mutate(d.id)} className="text-xs text-primary hover:underline">Refresh</button>
                )}
                <button type="button" onClick={() => removeDataset.mutate(d.id)} className="text-xs text-danger hover:underline">Delete</button>
              </div>
            </div>
          ))}
          {datasets.length === 0 && <p className="text-sm text-body/60">No datasets yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}

function RiskTab() {
  const { data, isLoading, isError, error } = useHeritageRiskRecords({ pageSize: 50 });
  const { createRiskRecord, removeRiskRecord } = useHeritageDatabaseMutations();
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ title: '', description: '', category: 'CraftDecline', level: 'Medium' });

  const records = data?.items || [];

  const handleCreate = (event) => {
    event.preventDefault();
    createRiskRecord.mutate(form, { onSuccess: () => { setShowForm(false); setForm({ title: '', description: '', category: 'CraftDecline', level: 'Medium' }); } });
  };

  return (
    <div>
      <div className="mb-4 flex justify-end">
        <Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'New Risk Record'}</Button>
      </div>

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <input required placeholder="Title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <input placeholder="Category" value={form.category} onChange={(e) => setForm((p) => ({ ...p, category: e.target.value }))} className={inputClass} />
          <select value={form.level} onChange={(e) => setForm((p) => ({ ...p, level: e.target.value }))} className={inputClass}>
            {riskLevels.map((l) => <option key={l} value={l}>{l}</option>)}
          </select>
          <textarea required rows={2} placeholder="Description" value={form.description} onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={createRiskRecord.isPending}>Log Risk Record</Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-2">
          {records.map((r) => (
            <div key={r.id} className="flex flex-wrap items-center justify-between gap-2 rounded-xl border border-border bg-surface p-4">
              <div>
                <p className="text-sm font-semibold text-heading">{r.title}</p>
                <p className="text-xs text-body/60">{r.category}{r.craftName ? ` · ${r.craftName}` : ''}{r.affectedArtisanCount ? ` · ${r.affectedArtisanCount} artisans affected` : ''}</p>
              </div>
              <div className="flex items-center gap-2">
                <Badge tone={riskLevelTone[r.level] || 'neutral'}>{r.level}</Badge>
                <button type="button" onClick={() => removeRiskRecord.mutate(r.id)} className="text-xs text-danger hover:underline">Delete</button>
              </div>
            </div>
          ))}
          {records.length === 0 && <p className="text-sm text-body/60">No risk records logged yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}

const tabs = [
  { key: 'overview', label: 'Overview' },
  { key: 'datasets', label: 'Datasets' },
  { key: 'risk', label: 'Risk Assessment' },
];

export default function HeritageDatabase() {
  const [tab, setTab] = useState('overview');

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Innovation Hub', path: routePaths.research },
          { label: 'Heritage Database' },
        ]}
        title="Heritage Database"
        description="Open datasets on districts, villages, crafts, producers, and heritage risk assessments."
      />

      <div className="mb-6 flex flex-wrap gap-2 border-b border-border">
        {tabs.map((t) => (
          <button
            key={t.key}
            type="button"
            onClick={() => setTab(t.key)}
            className={`border-b-2 px-3 py-2 text-sm font-medium ${tab === t.key ? 'border-primary text-primary' : 'border-transparent text-body/60'}`}
          >
            {t.label}
          </button>
        ))}
      </div>

      {tab === 'overview' && <OverviewTab />}
      {tab === 'datasets' && <DatasetsTab />}
      {tab === 'risk' && <RiskTab />}
    </div>
  );
}
