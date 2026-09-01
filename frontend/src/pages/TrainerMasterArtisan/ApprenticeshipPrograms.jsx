import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useHeritageSkills } from '../../hooks/useHeritageSkills';
import { useMyApprenticeshipPrograms, useApprenticeshipProgram, useApprenticeshipProgramMutations } from '../../hooks/useApprenticeshipPrograms';
import { useApprenticeEnrollmentsByProgram } from '../../hooks/useApprenticeEnrollments';

function EnrollmentsRoster({ programId }) {
  const enrollmentsQuery = useApprenticeEnrollmentsByProgram(programId);
  const enrollments = enrollmentsQuery.data || [];

  return (
    <div>
      <h4 className="mb-2 text-sm font-semibold text-heading">Enrolled Apprentices ({enrollments.length})</h4>
      <div className="space-y-1 text-xs text-body/60">
        {enrollments.map((e) => (
          <p key={e.id}>{e.apprenticeName} — {e.status} · {e.progressPercent}% complete</p>
        ))}
        {enrollments.length === 0 && <p>No apprentices enrolled yet.</p>}
      </div>
    </div>
  );
}

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const types = ['Apprenticeship', 'Workshop', 'Internship', 'Fellowship'];
const statusTone = { Draft: 'neutral', Published: 'success', Closed: 'neutral' };

const emptyForm = {
  type: 'Apprenticeship', title: '', description: '', heritageSkillId: '',
  location: '', durationWeeks: '', capacity: '', eligibilityRequirements: '', startDate: '', endDate: '',
};

function ProgramDetail({ id }) {
  const detailQuery = useApprenticeshipProgram(id);
  const { publish, close, remove, addMilestone, removeMilestone } = useApprenticeshipProgramMutations();
  const [milestone, setMilestone] = useState({ title: '', description: '' });

  const program = detailQuery.data;
  if (detailQuery.isLoading) return <p className="py-4 text-sm text-body/60">Loading…</p>;
  if (!program) return null;

  const handleAddMilestone = (event) => {
    event.preventDefault();
    if (!milestone.title) return;
    addMilestone.mutate(
      { id, payload: { ...milestone, displayOrder: program.milestones.length } },
      { onSuccess: () => setMilestone({ title: '', description: '' }) },
    );
  };

  return (
    <div className="mt-4 space-y-4 border-t border-border pt-4">
      <div className="flex flex-wrap gap-2">
        {program.status === 'Draft' && (
          <Button size="sm" variant="primary" disabled={publish.isPending} onClick={() => publish.mutate(id)}>Publish</Button>
        )}
        {program.status === 'Published' && (
          <Button size="sm" variant="secondary" disabled={close.isPending} onClick={() => close.mutate(id)}>Close</Button>
        )}
        <button type="button" onClick={() => remove.mutate(id)} className="text-xs text-danger hover:underline">Delete</button>
      </div>

      <div>
        <h4 className="mb-2 text-sm font-semibold text-heading">Milestones ({program.milestones.length})</h4>
        <div className="mb-2 space-y-1 text-xs text-body/60">
          {program.milestones.map((m) => (
            <div key={m.id} className="flex items-center justify-between rounded-lg border border-border px-3 py-2">
              <span>{m.title}</span>
              <button type="button" onClick={() => removeMilestone.mutate({ id, milestoneId: m.id })} className="text-danger hover:underline">Remove</button>
            </div>
          ))}
          {program.milestones.length === 0 && <p>No milestones yet.</p>}
        </div>
        <form onSubmit={handleAddMilestone} className="flex flex-wrap gap-2">
          <input placeholder="Milestone title" value={milestone.title} onChange={(e) => setMilestone((p) => ({ ...p, title: e.target.value }))} className={`${inputClass} flex-1`} />
          <input placeholder="Description" value={milestone.description} onChange={(e) => setMilestone((p) => ({ ...p, description: e.target.value }))} className={`${inputClass} flex-1`} />
          <Button type="submit" variant="secondary" size="sm" disabled={addMilestone.isPending}>Add</Button>
        </form>
      </div>

      <EnrollmentsRoster programId={id} />
    </div>
  );
}

export default function ApprenticeshipPrograms() {
  const { data, isLoading, isError, error } = useMyApprenticeshipPrograms();
  const skillsQuery = useHeritageSkills();
  const { create } = useApprenticeshipProgramMutations();
  const [showForm, setShowForm] = useState(false);
  const [expandedId, setExpandedId] = useState(null);
  const [form, setForm] = useState(emptyForm);

  const programs = data || [];

  const handleCreate = (event) => {
    event.preventDefault();
    create.mutate(
      {
        ...form,
        heritageSkillId: form.heritageSkillId || null,
        durationWeeks: form.durationWeeks === '' ? null : Number(form.durationWeeks),
        capacity: form.capacity === '' ? null : Number(form.capacity),
        startDate: form.startDate || null,
        endDate: form.endDate || null,
      },
      { onSuccess: () => { setShowForm(false); setForm(emptyForm); } },
    );
  };

  return (
    <div>
      <PageHeader
        title="Apprenticeship Programs"
        description="Create and manage the apprenticeship, workshop and fellowship programs you run."
        action={<Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'New Program'}</Button>}
      />

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <select value={form.type} onChange={(e) => setForm((p) => ({ ...p, type: e.target.value }))} className={inputClass}>
            {types.map((t) => <option key={t} value={t}>{t}</option>)}
          </select>
          <select value={form.heritageSkillId} onChange={(e) => setForm((p) => ({ ...p, heritageSkillId: e.target.value }))} className={inputClass}>
            <option value="">Skill (optional)</option>
            {(skillsQuery.data || []).map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}
          </select>
          <input required placeholder="Title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <textarea required rows={2} placeholder="Description" value={form.description} onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <input placeholder="Location" value={form.location} onChange={(e) => setForm((p) => ({ ...p, location: e.target.value }))} className={inputClass} />
          <input type="number" min="1" placeholder="Duration (weeks)" value={form.durationWeeks} onChange={(e) => setForm((p) => ({ ...p, durationWeeks: e.target.value }))} className={inputClass} />
          <input type="number" min="1" placeholder="Capacity" value={form.capacity} onChange={(e) => setForm((p) => ({ ...p, capacity: e.target.value }))} className={inputClass} />
          <div className="grid grid-cols-2 gap-3">
            <input type="date" value={form.startDate} onChange={(e) => setForm((p) => ({ ...p, startDate: e.target.value }))} className={inputClass} />
            <input type="date" value={form.endDate} onChange={(e) => setForm((p) => ({ ...p, endDate: e.target.value }))} className={inputClass} />
          </div>
          <textarea rows={2} placeholder="Eligibility requirements" value={form.eligibilityRequirements} onChange={(e) => setForm((p) => ({ ...p, eligibilityRequirements: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={create.isPending}>
            {create.isPending ? 'Creating…' : 'Create Program'}
          </Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {programs.map((p) => (
            <div key={p.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{p.title}</p>
                  <p className="text-xs text-body/60">
                    {p.type}{p.location ? ` · ${p.location}` : ''}{p.durationWeeks ? ` · ${p.durationWeeks}w` : ''} · {p.activeEnrollmentCount} enrolled
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={statusTone[p.status] || 'neutral'}>{p.status}</Badge>
                  <Button variant="secondary" onClick={() => setExpandedId(expandedId === p.id ? null : p.id)}>
                    {expandedId === p.id ? 'Hide' : 'Manage'}
                  </Button>
                </div>
              </div>
              {expandedId === p.id && <ProgramDetail id={p.id} />}
            </div>
          ))}
          {programs.length === 0 && <p className="text-sm text-body/60">No programs yet. Create your first one above.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
