import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import CraftHeritageCatalog from '../../components/heritage/CraftHeritageCatalog';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Table, SearchBar, Badge, Button, AsyncState } from '../../components/ui';
import { useDistricts } from '../../hooks/useDistricts';
import { useVillages } from '../../hooks/useVillages';
import { useHeritageDbSummary, useHeritageDatasets, useHeritageRiskRecords, useHeritageTourism, useHeritageDemographics, useHeritageExportAnalytics, useHeritageDatabaseMutations } from '../../hooks/useHeritageDatabase';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const datasetCategories = ['Producers', 'Products', 'Villages', 'Tourism', 'Crafts', 'Demographics', 'Other'];
const riskLevels = ['Low', 'Medium', 'High', 'Critical'];
const riskLevelTone = { Low: 'success', Medium: 'secondary', High: 'primary', Critical: 'neutral' };

function OverviewTab() {
  const [query, setQuery] = useState('');
  const districtsQuery = useDistricts();
  const villagesQuery = useVillages();
  const summaryQuery = useHeritageDbSummary({});

  const villageCountByDistrict = (villagesQuery.data || []).reduce((acc, v) => {
    acc[v.districtId] = (acc[v.districtId] || 0) + 1;
    return acc;
  }, {});

  const rows = (districtsQuery.data || []).filter(d => [d.name, d.division].join(' ').toLowerCase().includes(query.trim().toLowerCase())).map((d) => ({
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
        <SearchBar placeholder="Search districts or divisions…" value={query} onChange={e => setQuery(e.target.value)} />
      </div>
      <AsyncState isLoading={districtsQuery.isLoading} isError={districtsQuery.isError} error={districtsQuery.error}>
        <Table columns={['district', 'division', 'villages']} rows={rows} />
      </AsyncState>
    </div>
  );
}

function ExportAnalytics({ id }) {
  const { data, isLoading, isError, error } = useHeritageExportAnalytics(id);
  return (
    <div className="w-full border-t border-border pt-3">
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        {data && (
          <div className="space-y-1 text-xs text-body/70">
            <p>{data.totalExports} exports ({data.completedExports} completed) · {data.totalRowsExported} rows exported{data.lastExportedAt ? ` · last ${new Date(data.lastExportedAt).toLocaleDateString()}` : ''}</p>
            {data.byFormat.length > 0 && <p>By format: {data.byFormat.map((b) => `${b.label} (${b.count})`).join(', ')}</p>}
            {data.byMonth.length > 0 && <p>By month: {data.byMonth.map((b) => `${b.label} (${b.count})`).join(', ')}</p>}
            {data.topExporters.length > 0 && <p>Top exporters: {data.topExporters.map((b) => `${b.label} (${b.count})`).join(', ')}</p>}
          </div>
        )}
      </AsyncState>
    </div>
  );
}

function DatasetsTab() {
  const { data, isLoading, isError, error } = useHeritageDatasets({ pageSize: 50 });
  const { createDataset, refreshDataset, removeDataset, exportDataset } = useHeritageDatabaseMutations();
  const [analyticsId, setAnalyticsId] = useState(null);
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
          <input aria-label="Dataset name" required placeholder="Dataset name" value={form.name} onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <select aria-label="Category" value={form.category} onChange={(e) => setForm((p) => ({ ...p, category: e.target.value }))} className={inputClass}>
            {datasetCategories.map((c) => <option key={c} value={c}>{c}</option>)}
          </select>
          <label className="flex items-center gap-2 text-sm text-body/70">
            <input type="checkbox" checked={form.isLive} onChange={(e) => setForm((p) => ({ ...p, isLive: e.target.checked }))} /> Live-updating
          </label>
          <textarea aria-label="Description" required rows={2} placeholder="Description" value={form.description} onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
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
                <button type="button" onClick={() => exportDataset.mutate({ id: d.id, format: 'Csv' })} className="text-xs text-primary hover:underline">Export CSV</button>
                <button type="button" onClick={() => setAnalyticsId(analyticsId === d.id ? null : d.id)} className="text-xs text-primary hover:underline">{analyticsId === d.id ? 'Hide analytics' : 'Export analytics'}</button>
                <button type="button" onClick={() => removeDataset.mutate(d.id)} className="text-xs text-danger hover:underline">Delete</button>
              </div>
              {analyticsId === d.id && <ExportAnalytics id={d.id} />}
            </div>
          ))}
          {datasets.length === 0 && <p className="text-sm text-body/60">No datasets yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}

function BucketList({ title, buckets }) {
  return (
    <div className="rounded-xl border border-border bg-surface p-4">
      <p className="mb-2 text-sm font-semibold text-heading">{title}</p>
      <ul className="space-y-1 text-xs text-body/70">
        {(buckets || []).slice(0, 10).map((b) => (
          <li key={b.key} className="flex justify-between"><span>{b.label}</span><span className="font-medium text-heading">{b.count}</span></li>
        ))}
        {(buckets || []).length === 0 && <li>No data.</li>}
      </ul>
    </div>
  );
}

function DemographicsTab() {
  const { data, isLoading, isError, error } = useHeritageDemographics();
  return (
    <AsyncState isLoading={isLoading} isError={isError} error={error}>
      {data && (
        <div>
          <div className="mb-6 grid grid-cols-2 gap-3 sm:grid-cols-4">
            <div className="rounded-lg border border-border bg-surface p-3 text-center"><p className="text-lg font-semibold text-primary">{data.totalProducers}</p><p className="text-xs text-body/60">Producers</p></div>
            <div className="rounded-lg border border-border bg-surface p-3 text-center"><p className="text-lg font-semibold text-primary">{data.withHeritageIdentity}</p><p className="text-xs text-body/60">With heritage identity</p></div>
            <div className="rounded-lg border border-border bg-surface p-3 text-center"><p className="text-lg font-semibold text-primary">{data.verifiedHeritageIdentity}</p><p className="text-xs text-body/60">Verified</p></div>
            <div className="rounded-lg border border-border bg-surface p-3 text-center"><p className="text-lg font-semibold text-primary">{Number(data.averageYearsOfExperience).toFixed(1)}</p><p className="text-xs text-body/60">Avg. years experience</p></div>
          </div>
          <div className="grid gap-4 sm:grid-cols-2">
            <BucketList title="By division" buckets={data.byDivision} />
            <BucketList title="By district" buckets={data.byDistrict} />
            <BucketList title="By primary craft" buckets={data.byPrimaryCraft} />
            <BucketList title="By experience band" buckets={data.byExperienceBand} />
            <BucketList title="By verification status" buckets={data.byVerificationStatus} />
          </div>
        </div>
      )}
    </AsyncState>
  );
}

function TourismTab() {
  const [search, setSearch] = useState('');
  const { data, isLoading, isError, error } = useHeritageTourism({ pageSize: 50, search: search || undefined });
  const rows = (data?.items || []).map((t) => ({
    title: t.title,
    type: t.type,
    district: t.districtName,
    producer: t.producerName,
    price: t.price,
    rating: `${Number(t.averageRating).toFixed(1)} (${t.reviewCount})`,
  }));
  return (
    <div>
      <div className="mb-4 max-w-xl">
        <SearchBar placeholder="Search tourism records…" value={search} onChange={(e) => setSearch(e.target.value)} />
      </div>
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <Table columns={['title', 'type', 'district', 'producer', 'price', 'rating']} rows={rows} />
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
          <input aria-label="Title" required placeholder="Title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <input aria-label="Category" placeholder="Category" value={form.category} onChange={(e) => setForm((p) => ({ ...p, category: e.target.value }))} className={inputClass} />
          <select aria-label="Level" value={form.level} onChange={(e) => setForm((p) => ({ ...p, level: e.target.value }))} className={inputClass}>
            {riskLevels.map((l) => <option key={l} value={l}>{l}</option>)}
          </select>
          <textarea aria-label="Description" required rows={2} placeholder="Description" value={form.description} onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
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
  { key: 'crafts', label: 'Craft heritage' },
  { key: 'overview', label: 'Overview' },
  { key: 'datasets', label: 'Datasets' },
  { key: 'tourism', label: 'Tourism Data' },
  { key: 'demographics', label: 'Producer Demographics' },
  { key: 'risk', label: 'Risk Assessment' },
];

export default function HeritageDatabase() {
  const [params, setParams] = useSearchParams();
  const tab = tabs.some(t => t.key === params.get('tab')) ? params.get('tab') : 'crafts';
  const setTab = (key) => { const next = new URLSearchParams(params); next.set('tab', key); next.delete('craft'); setParams(next); };

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Innovation Hub', path: routePaths.researcher },
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
            aria-pressed={tab === t.key}
            onClick={() => setTab(t.key)}
            className={`border-b-2 px-3 py-2 text-sm font-medium ${tab === t.key ? 'border-primary text-primary' : 'border-transparent text-body/60'}`}
          >
            {t.label}
          </button>
        ))}
      </div>

      {tab === 'crafts' && <CraftHeritageCatalog />}
      {tab === 'overview' && <OverviewTab />}
      {tab === 'datasets' && <DatasetsTab />}
      {tab === 'tourism' && <TourismTab />}
      {tab === 'demographics' && <DemographicsTab />}
      {tab === 'risk' && <RiskTab />}
    </div>
  );
}
