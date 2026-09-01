import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useInnovationPrototypes, useInnovationPrototype, usePrototypeIssues, useInnovationPrototypeMutations } from '../../hooks/useInnovationPrototypes';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const statusTone = { Concept: 'neutral', InDevelopment: 'primary', Testing: 'secondary', Ready: 'success', Retired: 'neutral' };
const issueSeverityTone = { Low: 'neutral', Medium: 'secondary', High: 'primary', Critical: 'neutral' };

function PrototypeDetail({ id }) {
  const detailQuery = useInnovationPrototype(id);
  const issuesQuery = usePrototypeIssues(id);
  const { addIteration, addTestCase, removeTestCase, addIssue, updateIssue } = useInnovationPrototypeMutations();
  const [iteration, setIteration] = useState({ label: '', changeSummary: '' });
  const [testCase, setTestCase] = useState({ title: '', expectedResult: '' });
  const [issue, setIssue] = useState({ title: '', description: '', severity: 'Medium' });

  const prototype = detailQuery.data;
  if (detailQuery.isLoading) return <p className="py-4 text-sm text-body/60">Loading…</p>;
  if (!prototype) return null;

  const issues = issuesQuery.data || [];

  return (
    <div className="mt-4 grid gap-4 border-t border-border pt-4 sm:grid-cols-2">
      <div>
        <h4 className="mb-2 text-sm font-semibold text-heading">Iterations ({prototype.iterations.length})</h4>
        <div className="mb-2 space-y-1 text-xs text-body/60">
          {prototype.iterations.map((v) => (
            <p key={v.id}>v{v.versionNumber} {v.label ? `(${v.label})` : ''}{v.isCurrent ? ' · current' : ''} — {v.changeSummary}</p>
          ))}
          {prototype.iterations.length === 0 && <p>No iterations yet.</p>}
        </div>
        <form onSubmit={(e) => { e.preventDefault(); if (!iteration.changeSummary) return; addIteration.mutate({ id, payload: iteration }, { onSuccess: () => setIteration({ label: '', changeSummary: '' }) }); }} className="flex flex-wrap gap-2">
          <input placeholder="Label" value={iteration.label} onChange={(e) => setIteration((p) => ({ ...p, label: e.target.value }))} className={`${inputClass} w-24`} />
          <input placeholder="Change summary" value={iteration.changeSummary} onChange={(e) => setIteration((p) => ({ ...p, changeSummary: e.target.value }))} className={`${inputClass} flex-1`} />
          <Button type="submit" variant="secondary" size="sm" disabled={addIteration.isPending}>Add</Button>
        </form>

        <h4 className="mb-2 mt-4 text-sm font-semibold text-heading">Test Cases ({prototype.testCases.length})</h4>
        <div className="mb-2 space-y-1 text-xs text-body/60">
          {prototype.testCases.map((t) => (
            <div key={t.id} className="flex items-center justify-between rounded-lg border border-border px-3 py-2">
              <span>{t.title} ({t.priority})</span>
              <button type="button" onClick={() => removeTestCase.mutate({ id, testCaseId: t.id })} className="text-danger hover:underline">Remove</button>
            </div>
          ))}
          {prototype.testCases.length === 0 && <p>No test cases yet.</p>}
        </div>
        <form onSubmit={(e) => { e.preventDefault(); if (!testCase.title || !testCase.expectedResult) return; addTestCase.mutate({ id, payload: { ...testCase, orderIndex: prototype.testCases.length } }, { onSuccess: () => setTestCase({ title: '', expectedResult: '' }) }); }} className="flex flex-wrap gap-2">
          <input placeholder="Test title" value={testCase.title} onChange={(e) => setTestCase((p) => ({ ...p, title: e.target.value }))} className={`${inputClass} flex-1`} />
          <input placeholder="Expected result" value={testCase.expectedResult} onChange={(e) => setTestCase((p) => ({ ...p, expectedResult: e.target.value }))} className={`${inputClass} flex-1`} />
          <Button type="submit" variant="secondary" size="sm" disabled={addTestCase.isPending}>Add</Button>
        </form>
      </div>

      <div>
        <h4 className="mb-2 text-sm font-semibold text-heading">Issues ({issues.length})</h4>
        <div className="mb-2 space-y-1">
          {issues.map((i) => (
            <div key={i.id} className="flex items-center justify-between rounded-lg border border-border px-3 py-2 text-xs">
              <span>{i.title}</span>
              <div className="flex items-center gap-2">
                <Badge tone={issueSeverityTone[i.severity] || 'neutral'}>{i.severity}</Badge>
                {i.status !== 'Resolved' && (
                  <button type="button" onClick={() => updateIssue.mutate({ id, issueId: i.id, payload: { title: i.title, description: i.description, severity: i.severity, status: 'Resolved' } })} className="text-primary hover:underline">
                    Resolve
                  </button>
                )}
              </div>
            </div>
          ))}
          {issues.length === 0 && <p className="text-xs text-body/50">No issues reported.</p>}
        </div>
        <form onSubmit={(e) => { e.preventDefault(); if (!issue.title) return; addIssue.mutate({ id, payload: issue }, { onSuccess: () => setIssue({ title: '', description: '', severity: 'Medium' }) }); }} className="flex flex-wrap gap-2">
          <input placeholder="Issue title" value={issue.title} onChange={(e) => setIssue((p) => ({ ...p, title: e.target.value }))} className={`${inputClass} flex-1`} />
          <select value={issue.severity} onChange={(e) => setIssue((p) => ({ ...p, severity: e.target.value }))} className={inputClass}>
            {['Low', 'Medium', 'High', 'Critical'].map((s) => <option key={s} value={s}>{s}</option>)}
          </select>
          <Button type="submit" variant="secondary" size="sm" disabled={addIssue.isPending}>Report</Button>
        </form>
      </div>
    </div>
  );
}

export default function InnovationPrototypes() {
  const { data, isLoading, isError, error } = useInnovationPrototypes({ pageSize: 50 });
  const { create } = useInnovationPrototypeMutations();
  const [showForm, setShowForm] = useState(false);
  const [expandedId, setExpandedId] = useState(null);
  const [form, setForm] = useState({ name: '', description: '', category: '' });

  const prototypes = data?.items || [];

  const handleCreate = (event) => {
    event.preventDefault();
    create.mutate(form, { onSuccess: () => { setShowForm(false); setForm({ name: '', description: '', category: '' }); } });
  };

  return (
    <div>
      <PageHeader
        title="Innovation Prototypes"
        description="Prototype heritage-tech ideas: track iterations, test cases and issues."
        action={<Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'New Prototype'}</Button>}
      />

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <input required placeholder="Prototype name" value={form.name} onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <input placeholder="Category" value={form.category} onChange={(e) => setForm((p) => ({ ...p, category: e.target.value }))} className={inputClass} />
          <textarea required rows={2} placeholder="Description" value={form.description} onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={create.isPending}>{create.isPending ? 'Creating…' : 'Create Prototype'}</Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {prototypes.map((p) => (
            <div key={p.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{p.name}</p>
                  <p className="text-xs text-body/60">{p.category || 'Uncategorized'} · {p.versionCount} iteration(s) · {p.testCaseCount} test case(s) · {p.openIssueCount} open issue(s)</p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={statusTone[p.status] || 'neutral'}>{p.status}</Badge>
                  <Button variant="secondary" onClick={() => setExpandedId(expandedId === p.id ? null : p.id)}>
                    {expandedId === p.id ? 'Hide' : 'Manage'}
                  </Button>
                </div>
              </div>
              {expandedId === p.id && <PrototypeDetail id={p.id} />}
            </div>
          ))}
          {prototypes.length === 0 && <p className="text-sm text-body/60">No prototypes yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
