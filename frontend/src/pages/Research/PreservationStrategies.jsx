import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { usePreservationStrategies, usePreservationStrategy, usePreservationStrategyMutations } from '../../hooks/usePreservationStrategies';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const statusTone = { Proposed: 'neutral', Active: 'primary', OnHold: 'secondary', Completed: 'success', Archived: 'neutral' };

const emptyForm = { title: '', heritageProblem: '', proposedSolution: '', expectedImpact: '' };

function StrategyDetail({ id }) {
  const detailQuery = usePreservationStrategy(id);
  const { updateAction, addObjective, addAction, removeObjective, removeAction } = usePreservationStrategyMutations();
  const [objTitle, setObjTitle] = useState('');
  const [actionForm, setActionForm] = useState({ title: '', objectiveId: '' });

  const strategy = detailQuery.data;
  if (detailQuery.isLoading) return <p className="py-4 text-sm text-body/60">Loading…</p>;
  if (!strategy) return null;

  const handleAddObjective = (e) => {
    e.preventDefault();
    if (!objTitle) return;
    addObjective.mutate({ id, payload: { title: objTitle, orderIndex: strategy.objectives.length } }, { onSuccess: () => setObjTitle('') });
  };

  const handleAddAction = (e) => {
    e.preventDefault();
    if (!actionForm.title) return;
    addAction.mutate(
      { id, payload: { title: actionForm.title, strategyObjectiveId: actionForm.objectiveId || null, orderIndex: strategy.actions.length } },
      { onSuccess: () => setActionForm({ title: '', objectiveId: '' }) },
    );
  };

  return (
    <div className="mt-4 grid gap-4 border-t border-border pt-4 sm:grid-cols-2">
      <div>
        <h4 className="mb-2 text-sm font-semibold text-heading">Objectives</h4>
        <div className="mb-2 space-y-1">
          {strategy.objectives.map((o) => (
            <div key={o.id} className="flex items-center justify-between rounded-lg border border-border px-3 py-2 text-xs">
              <span>{o.title}</span>
              <div className="flex items-center gap-2">
                {o.isAchieved && <Badge tone="success">Achieved</Badge>}
                <button type="button" onClick={() => removeObjective.mutate({ id, objectiveId: o.id })} className="text-danger hover:underline">Remove</button>
              </div>
            </div>
          ))}
          {strategy.objectives.length === 0 && <p className="text-xs text-body/50">No objectives yet.</p>}
        </div>
        <form onSubmit={handleAddObjective} className="flex gap-2">
          <input placeholder="Objective title" value={objTitle} onChange={(e) => setObjTitle(e.target.value)} className={`${inputClass} flex-1`} />
          <Button type="submit" variant="secondary" size="sm" disabled={addObjective.isPending}>Add</Button>
        </form>
      </div>

      <div>
        <h4 className="mb-2 text-sm font-semibold text-heading">Actions</h4>
        <div className="mb-2 space-y-1">
          {strategy.actions.map((a) => (
            <div key={a.id} className="flex items-center justify-between rounded-lg border border-border px-3 py-2 text-xs">
              <span>{a.title}{a.dueDate ? ` · due ${new Date(a.dueDate).toLocaleDateString()}` : ''}</span>
              <div className="flex items-center gap-2">
                <Badge tone={a.status === 'Done' ? 'success' : 'neutral'}>{a.status}</Badge>
                {a.status !== 'Done' && (
                  <button type="button" onClick={() => updateAction.mutate({ id, actionId: a.id, payload: { title: a.title, status: 'Done', orderIndex: a.orderIndex, strategyObjectiveId: a.strategyObjectiveId } })} className="text-primary hover:underline">Complete</button>
                )}
                <button type="button" onClick={() => removeAction.mutate({ id, actionId: a.id })} className="text-danger hover:underline">Remove</button>
              </div>
            </div>
          ))}
          {strategy.actions.length === 0 && <p className="text-xs text-body/50">No actions yet.</p>}
        </div>
        <form onSubmit={handleAddAction} className="flex flex-wrap gap-2">
          <input placeholder="Action title" value={actionForm.title} onChange={(e) => setActionForm((p) => ({ ...p, title: e.target.value }))} className={`${inputClass} flex-1`} />
          <select value={actionForm.objectiveId} onChange={(e) => setActionForm((p) => ({ ...p, objectiveId: e.target.value }))} className={inputClass}>
            <option value="">No objective</option>
            {strategy.objectives.map((o) => <option key={o.id} value={o.id}>{o.title}</option>)}
          </select>
          <Button type="submit" variant="secondary" size="sm" disabled={addAction.isPending}>Add</Button>
        </form>
      </div>
    </div>
  );
}

export default function PreservationStrategies() {
  const { data, isLoading, isError, error } = usePreservationStrategies({ pageSize: 50 });
  const { create } = usePreservationStrategyMutations();
  const [showForm, setShowForm] = useState(false);
  const [expandedId, setExpandedId] = useState(null);
  const [form, setForm] = useState(emptyForm);

  const strategies = data?.items || [];

  const handleCreate = (event) => {
    event.preventDefault();
    create.mutate(form, { onSuccess: () => { setShowForm(false); setForm(emptyForm); } });
  };

  return (
    <div>
      <PageHeader
        title="Preservation Strategies"
        description="Propose and manage heritage preservation strategies with objectives and action plans."
        action={<Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'New Strategy'}</Button>}
      />

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <input required placeholder="Title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <textarea required rows={2} placeholder="Heritage problem" value={form.heritageProblem} onChange={(e) => setForm((p) => ({ ...p, heritageProblem: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <textarea required rows={2} placeholder="Proposed solution" value={form.proposedSolution} onChange={(e) => setForm((p) => ({ ...p, proposedSolution: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <textarea rows={2} placeholder="Expected impact" value={form.expectedImpact} onChange={(e) => setForm((p) => ({ ...p, expectedImpact: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={create.isPending}>{create.isPending ? 'Creating…' : 'Create Strategy'}</Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {strategies.map((s) => (
            <div key={s.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{s.title}</p>
                  <p className="text-xs text-body/60">{s.objectiveCount} objective(s) · {s.completedActionCount}/{s.actionCount} action(s) done</p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={statusTone[s.status] || 'neutral'}>{s.status}</Badge>
                  <Button variant="secondary" onClick={() => setExpandedId(expandedId === s.id ? null : s.id)}>
                    {expandedId === s.id ? 'Hide' : 'Manage'}
                  </Button>
                </div>
              </div>
              {expandedId === s.id && <StrategyDetail id={s.id} />}
            </div>
          ))}
          {strategies.length === 0 && <p className="text-sm text-body/60">No preservation strategies yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
