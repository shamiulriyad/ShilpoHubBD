import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { confirmAction } from '../../lib/confirm';
import {
  usePolicySimulations, useComplianceRecords, useComplianceRecord, usePolicyComplianceMutations,
} from '../../hooks/usePolicyCompliance';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const simulationTypes = ['GrantProgram', 'TrainingProgram', 'TourismCampaign', 'ExportStrategy', 'EmploymentPrediction'];
const scopes = ['National', 'District', 'Village', 'Craft'];
const entityTypes = ['Producer', 'Village', 'District', 'Product', 'Organization'];
const complianceStatusTone = { NotStarted: 'neutral', InProgress: 'secondary', Compliant: 'success', NonCompliant: 'neutral', Waived: 'neutral', Expired: 'neutral' };

function SimulationsTab() {
  const { data, isLoading, isError, error } = usePolicySimulations({ pageSize: 50 });
  const { runSimulation, removeSimulation } = usePolicyComplianceMutations();
  const [form, setForm] = useState({ title: '', simulationType: 'GrantProgram', scope: 'National', horizonMonths: 12, budget: '' });

  const simulations = data?.items || [];

  const handleRun = (event) => {
    event.preventDefault();
    runSimulation.mutate(
      { ...form, horizonMonths: Number(form.horizonMonths) || 12, budget: form.budget === '' ? null : Number(form.budget) },
      { onSuccess: () => setForm({ title: '', simulationType: 'GrantProgram', scope: 'National', horizonMonths: 12, budget: '' }) },
    );
  };

  return (
    <div>
      <form onSubmit={handleRun} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
        <input aria-label="Scenario title" required placeholder="Scenario title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
        <select aria-label="Simulation Type" value={form.simulationType} onChange={(e) => setForm((p) => ({ ...p, simulationType: e.target.value }))} className={inputClass}>
          {simulationTypes.map((t) => <option key={t} value={t}>{t}</option>)}
        </select>
        <select aria-label="Scope" value={form.scope} onChange={(e) => setForm((p) => ({ ...p, scope: e.target.value }))} className={inputClass}>
          {scopes.map((s) => <option key={s} value={s}>{s}</option>)}
        </select>
        <input aria-label="Horizon" type="number" min="3" max="120" placeholder="Horizon (months)" value={form.horizonMonths} onChange={(e) => setForm((p) => ({ ...p, horizonMonths: e.target.value }))} className={inputClass} />
        <input aria-label="Budget" type="number" min="0" placeholder="Budget (৳, optional)" value={form.budget} onChange={(e) => setForm((p) => ({ ...p, budget: e.target.value }))} className={inputClass} />
        <Button type="submit" variant="primary" className="sm:col-span-2" disabled={runSimulation.isPending}>
          {runSimulation.isPending ? 'Simulating…' : 'Run Simulation'}
        </Button>
      </form>

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-2">
          {simulations.map((s) => (
            <div key={s.id} className="flex items-center justify-between rounded-xl border border-border bg-surface p-4">
              <div>
                <p className="text-sm font-semibold text-heading">{s.title}</p>
                <p className="text-xs text-body/60">{s.simulationType} · {s.scopeLabel} · {s.horizonMonths}mo · confidence {s.confidence}</p>
              </div>
              <button type="button" onClick={async () => { if (await confirmAction('Remove this? This cannot be undone.', { confirmLabel: 'Yes, remove' })) removeSimulation.mutate(s.id); }} className="text-xs text-danger hover:underline">Delete</button>
            </div>
          ))}
          {simulations.length === 0 && <p className="text-sm text-body/60">No simulations run yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}

function ComplianceDetail({ id }) {
  const detailQuery = useComplianceRecord(id);
  const { upsertRequirement, removeRequirement } = usePolicyComplianceMutations();
  const [req, setReq] = useState({ code: '', title: '', status: 'Unmet', isMandatory: true });

  const record = detailQuery.data;
  if (detailQuery.isLoading) return <p className="py-2 text-xs text-body/60">Loading…</p>;
  if (detailQuery.isError) return <p role="alert" className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">Unable to load this record. It may have been removed or you may not have access.</p>;
  if (!record) return <p className="py-4 text-sm text-body/60">This record is unavailable.</p>;

  const handleAdd = (e) => {
    e.preventDefault();
    if (!req.code || !req.title) return;
    upsertRequirement.mutate({ id, payload: { ...req, displayOrder: record.requirements.length } }, { onSuccess: () => setReq({ code: '', title: '', status: 'Unmet', isMandatory: true }) });
  };

  return (
    <div className="mt-3 space-y-2 border-t border-border pt-3">
      {record.requirements.map((r) => (
        <div key={r.id} className="flex items-center justify-between rounded-lg border border-border px-3 py-2 text-xs">
          <span>{r.code} — {r.title}{r.isMandatory ? ' *' : ''}</span>
          <div className="flex items-center gap-2">
            <Badge tone={r.status === 'Met' ? 'success' : 'neutral'}>{r.status}</Badge>
            <button type="button" onClick={async () => { if (await confirmAction('Remove this? This cannot be undone.', { confirmLabel: 'Yes, remove' })) removeRequirement.mutate({ id, requirementId: r.id }); }} className="text-danger hover:underline">Remove</button>
          </div>
        </div>
      ))}
      <form onSubmit={handleAdd} className="flex flex-wrap gap-2">
        <input aria-label="Code" placeholder="Code" value={req.code} onChange={(e) => setReq((p) => ({ ...p, code: e.target.value }))} className={`${inputClass} w-24`} />
        <input aria-label="Requirement title" placeholder="Requirement title" value={req.title} onChange={(e) => setReq((p) => ({ ...p, title: e.target.value }))} className={`${inputClass} flex-1`} />
        <Button type="submit" variant="secondary" size="sm" disabled={upsertRequirement.isPending}>Add</Button>
      </form>
    </div>
  );
}

function ComplianceTab() {
  const { data, isLoading, isError, error } = useComplianceRecords({ pageSize: 50 });
  const { createComplianceRecord } = usePolicyComplianceMutations();
  const [showForm, setShowForm] = useState(false);
  const [expandedId, setExpandedId] = useState(null);
  const [form, setForm] = useState({ entityType: 'Producer', entityLabel: '', framework: '' });

  const records = data?.items || [];

  const handleCreate = (event) => {
    event.preventDefault();
    createComplianceRecord.mutate({ ...form, requirements: [] }, { onSuccess: () => { setShowForm(false); setForm({ entityType: 'Producer', entityLabel: '', framework: '' }); } });
  };

  return (
    <div>
      <div className="mb-4 flex justify-end">
        <Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'New Compliance Record'}</Button>
      </div>

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <select aria-label="Entity Type" value={form.entityType} onChange={(e) => setForm((p) => ({ ...p, entityType: e.target.value }))} className={inputClass}>
            {entityTypes.map((t) => <option key={t} value={t}>{t}</option>)}
          </select>
          <input aria-label="Entity label" required placeholder="Entity label" value={form.entityLabel} onChange={(e) => setForm((p) => ({ ...p, entityLabel: e.target.value }))} className={inputClass} />
          <input aria-label="Framework" required placeholder="Framework (e.g. Fair Trade)" value={form.framework} onChange={(e) => setForm((p) => ({ ...p, framework: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={createComplianceRecord.isPending}>Create Record</Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-2">
          {records.map((r) => (
            <div key={r.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{r.entityLabel} ({r.entityType})</p>
                  <p className="text-xs text-body/60">{r.framework} · {r.overallScorePercent}% score</p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={complianceStatusTone[r.status] || 'neutral'}>{r.status}</Badge>
                  <Button variant="secondary" onClick={() => setExpandedId(expandedId === r.id ? null : r.id)}>
                    {expandedId === r.id ? 'Hide' : 'Requirements'}
                  </Button>
                </div>
              </div>
              {expandedId === r.id && <ComplianceDetail id={r.id} />}
            </div>
          ))}
          {records.length === 0 && <p className="text-sm text-body/60">No compliance records yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}

export default function PolicyCompliance() {
  const [tab, setTab] = useState('simulations');

  return (
    <div>
      <PageHeader title="Policy Simulator & Compliance" description="Model policy scenarios and track compliance records against frameworks." />

      <div className="mb-4 flex gap-2 border-b border-border">
        <button type="button" onClick={() => setTab('simulations')} className={`border-b-2 px-3 py-2 text-sm font-medium ${tab === 'simulations' ? 'border-primary text-primary' : 'border-transparent text-body/60'}`}>Policy Simulator</button>
        <button type="button" onClick={() => setTab('compliance')} className={`border-b-2 px-3 py-2 text-sm font-medium ${tab === 'compliance' ? 'border-primary text-primary' : 'border-transparent text-body/60'}`}>Compliance</button>
      </div>

      {tab === 'simulations' ? <SimulationsTab /> : <ComplianceTab />}
    </div>
  );
}
