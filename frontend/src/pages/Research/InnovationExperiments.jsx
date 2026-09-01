import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useInnovationExperiments, useInnovationExperiment, useInnovationExperimentMutations } from '../../hooks/useInnovationExperiments';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const modelTypes = ['Classification', 'Regression', 'Clustering', 'ComputerVision', 'NLP', 'Recommendation', 'Other'];
const statusTone = { Draft: 'neutral', Active: 'primary', Paused: 'secondary', Completed: 'success', Archived: 'neutral' };
const runStatusTone = { Queued: 'neutral', Running: 'primary', Completed: 'success', Failed: 'neutral' };

const emptyForm = { name: '', objective: '', modelType: 'Classification', framework: '' };

function ExperimentDetail({ id }) {
  const detailQuery = useInnovationExperiment(id);
  const { createRun, updateRun } = useInnovationExperimentMutations();

  const experiment = detailQuery.data;
  if (detailQuery.isLoading) return <p className="py-4 text-sm text-body/60">Loading…</p>;
  if (!experiment) return null;

  return (
    <div className="mt-4 space-y-4 border-t border-border pt-4">
      <p className="text-xs text-body/60">{experiment.versionCount} version(s) · {experiment.runCount} run(s)</p>

      <div>
        <div className="mb-2 flex items-center justify-between">
          <h4 className="text-sm font-semibold text-heading">Training Runs</h4>
          <Button
            size="sm"
            variant="secondary"
            disabled={createRun.isPending}
            onClick={() => createRun.mutate({ id, payload: {} })}
          >
            {createRun.isPending ? 'Starting…' : 'Start New Run'}
          </Button>
        </div>
        <div className="space-y-2">
          {experiment.runs.map((r) => (
            <div key={r.id} className="flex items-center justify-between rounded-lg border border-border bg-surface px-3 py-2 text-sm">
              <span>
                Run #{r.runNumber}{r.primaryMetricName ? ` · ${r.primaryMetricName}: ${r.primaryMetricValue ?? '—'}` : ''}
              </span>
              <div className="flex items-center gap-2">
                <Badge tone={runStatusTone[r.status] || 'neutral'}>{r.status}</Badge>
                {r.status === 'Queued' && (
                  <button type="button" onClick={() => updateRun.mutate({ id, runId: r.id, payload: { status: 'Running', startedAt: new Date().toISOString() } })} className="text-xs text-primary hover:underline">
                    Start
                  </button>
                )}
                {r.status === 'Running' && (
                  <button type="button" onClick={() => updateRun.mutate({ id, runId: r.id, payload: { status: 'Completed', completedAt: new Date().toISOString() } })} className="text-xs text-primary hover:underline">
                    Complete
                  </button>
                )}
              </div>
            </div>
          ))}
          {experiment.runs.length === 0 && <p className="text-sm text-body/60">No training runs yet.</p>}
        </div>
      </div>

      <div>
        <h4 className="mb-2 text-sm font-semibold text-heading">Versions</h4>
        <div className="space-y-1 text-xs text-body/60">
          {experiment.versions.map((v) => (
            <p key={v.id}>v{v.versionNumber} {v.label ? `(${v.label})` : ''}{v.isCurrent ? ' · current' : ''} — {v.notes}</p>
          ))}
          {experiment.versions.length === 0 && <p>No versions yet.</p>}
        </div>
      </div>
    </div>
  );
}

export default function InnovationExperiments() {
  const { data, isLoading, isError, error } = useInnovationExperiments({ pageSize: 50 });
  const { create } = useInnovationExperimentMutations();
  const [showForm, setShowForm] = useState(false);
  const [expandedId, setExpandedId] = useState(null);
  const [form, setForm] = useState(emptyForm);

  const experiments = data?.items || [];

  const handleCreate = (event) => {
    event.preventDefault();
    create.mutate(form, { onSuccess: () => { setShowForm(false); setForm(emptyForm); } });
  };

  return (
    <div>
      <PageHeader
        title="Innovation Experiments"
        description="Run and track AI/ML experiments for heritage innovation — versions, configs and training runs."
        action={<Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'New Experiment'}</Button>}
      />

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <input required placeholder="Experiment name" value={form.name} onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <select value={form.modelType} onChange={(e) => setForm((p) => ({ ...p, modelType: e.target.value }))} className={inputClass}>
            {modelTypes.map((t) => <option key={t} value={t}>{t}</option>)}
          </select>
          <input placeholder="Framework (e.g. PyTorch)" value={form.framework} onChange={(e) => setForm((p) => ({ ...p, framework: e.target.value }))} className={inputClass} />
          <textarea required rows={2} placeholder="Objective" value={form.objective} onChange={(e) => setForm((p) => ({ ...p, objective: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={create.isPending}>{create.isPending ? 'Creating…' : 'Create Experiment'}</Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {experiments.map((e) => (
            <div key={e.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{e.name}</p>
                  <p className="text-xs text-body/60">{e.modelType}{e.framework ? ` · ${e.framework}` : ''} · {e.versionCount} version(s), {e.runCount} run(s)</p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={statusTone[e.status] || 'neutral'}>{e.status}</Badge>
                  <Button variant="secondary" onClick={() => setExpandedId(expandedId === e.id ? null : e.id)}>
                    {expandedId === e.id ? 'Hide' : 'Manage'}
                  </Button>
                </div>
              </div>
              {expandedId === e.id && <ExperimentDetail id={e.id} />}
            </div>
          ))}
          {experiments.length === 0 && <p className="text-sm text-body/60">No experiments yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
