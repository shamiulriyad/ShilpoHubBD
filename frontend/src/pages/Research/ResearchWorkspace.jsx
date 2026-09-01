import { useState } from 'react';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import {
  useResearchProjects, useResearchProject, useResearchProjectMutations, useResearchActivity,
  useResearchPapers, useResearchNotes, useResearchTasks, useResearchMilestones, useResearchWorkItemMutations,
  useProjectPublications,
} from '../../hooks/useResearchWorkspace';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const statusTone = { Planning: 'neutral', Active: 'primary', InProgress: 'primary', Review: 'secondary', Completed: 'success', Archived: 'neutral' };
const tabs = ['Overview', 'Papers', 'Notes', 'Tasks', 'Milestones', 'Publications', 'Members'];
const publicationTypes = ['JournalArticle', 'ConferencePaper', 'Report', 'CaseStudy', 'Preprint', 'Other'];

function OverviewTab({ project }) {
  const activityQuery = useResearchActivity(project.id, 20);
  return (
    <div className="space-y-4">
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
        {[
          ['Tasks', `${project.openTaskCount}/${project.taskCount} open`],
          ['Milestones', project.milestoneCount],
          ['Notes', project.noteCount],
          ['Papers', project.paperCount],
        ].map(([label, value]) => (
          <div key={label} className="rounded-lg border border-border bg-surface p-3 text-center">
            <p className="text-lg font-semibold text-primary">{value}</p>
            <p className="text-xs text-body/60">{label}</p>
          </div>
        ))}
      </div>
      <div>
        <h4 className="mb-2 text-sm font-semibold text-heading">Recent Activity</h4>
        <div className="space-y-1 text-xs text-body/60">
          {(activityQuery.data || []).map((a) => (
            <p key={a.id}>{a.actorName}: {a.summary} · {new Date(a.createdAt).toLocaleString()}</p>
          ))}
          {(activityQuery.data || []).length === 0 && <p>No activity yet.</p>}
        </div>
      </div>
    </div>
  );
}

function PapersTab({ projectId }) {
  const papersQuery = useResearchPapers(projectId);
  const { createPaper, removePaper } = useResearchWorkItemMutations(projectId);
  const [form, setForm] = useState({ title: '', abstract: '' });

  const handleAdd = (e) => {
    e.preventDefault();
    if (!form.title) return;
    createPaper.mutate(form, { onSuccess: () => setForm({ title: '', abstract: '' }) });
  };

  return (
    <div>
      <form onSubmit={handleAdd} className="mb-3 flex flex-wrap gap-2">
        <input placeholder="Paper title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className={`${inputClass} flex-1`} />
        <input placeholder="Abstract" value={form.abstract} onChange={(e) => setForm((p) => ({ ...p, abstract: e.target.value }))} className={`${inputClass} flex-1`} />
        <Button type="submit" variant="secondary" size="sm" disabled={createPaper.isPending}>Add</Button>
      </form>
      <div className="space-y-2">
        {(papersQuery.data || []).map((p) => (
          <div key={p.id} className="flex items-center justify-between rounded-lg border border-border bg-surface px-3 py-2 text-sm">
            <div>
              <p className="font-medium text-heading">{p.title}</p>
              <p className="text-xs text-body/60">{p.status}{p.targetVenue ? ` · ${p.targetVenue}` : ''}</p>
            </div>
            <button type="button" onClick={() => removePaper.mutate(p.id)} className="text-xs text-danger hover:underline">Remove</button>
          </div>
        ))}
        {(papersQuery.data || []).length === 0 && <p className="text-sm text-body/60">No papers yet.</p>}
      </div>
    </div>
  );
}

function NotesTab({ projectId }) {
  const notesQuery = useResearchNotes(projectId);
  const { createNote, removeNote } = useResearchWorkItemMutations(projectId);
  const [form, setForm] = useState({ title: '', content: '' });

  const handleAdd = (e) => {
    e.preventDefault();
    if (!form.title || !form.content) return;
    createNote.mutate(form, { onSuccess: () => setForm({ title: '', content: '' }) });
  };

  return (
    <div>
      <form onSubmit={handleAdd} className="mb-3 space-y-2">
        <input placeholder="Note title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className={`${inputClass} w-full`} />
        <textarea rows={2} placeholder="Content" value={form.content} onChange={(e) => setForm((p) => ({ ...p, content: e.target.value }))} className={`${inputClass} w-full`} />
        <Button type="submit" variant="secondary" size="sm" disabled={createNote.isPending}>Add Note</Button>
      </form>
      <div className="space-y-2">
        {(notesQuery.data || []).map((n) => (
          <div key={n.id} className="rounded-lg border border-border bg-surface p-3 text-sm">
            <div className="flex items-center justify-between">
              <p className="font-medium text-heading">{n.title}</p>
              <button type="button" onClick={() => removeNote.mutate(n.id)} className="text-xs text-danger hover:underline">Remove</button>
            </div>
            <p className="text-xs text-body/60">{n.content}</p>
          </div>
        ))}
        {(notesQuery.data || []).length === 0 && <p className="text-sm text-body/60">No notes yet.</p>}
      </div>
    </div>
  );
}

function TasksTab({ projectId }) {
  const tasksQuery = useResearchTasks(projectId);
  const milestonesQuery = useResearchMilestones(projectId);
  const { createTask, updateTaskStatus, removeTask } = useResearchWorkItemMutations(projectId);
  const [form, setForm] = useState({ title: '', priority: 'Medium', milestoneId: '' });
  const milestones = milestonesQuery.data;

  const handleAdd = (e) => {
    e.preventDefault();
    if (!form.title) return;
    createTask.mutate({ ...form, milestoneId: form.milestoneId || null }, { onSuccess: () => setForm({ title: '', priority: 'Medium', milestoneId: '' }) });
  };

  return (
    <div>
      <form onSubmit={handleAdd} className="mb-3 flex flex-wrap gap-2">
        <input placeholder="Task title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className={`${inputClass} flex-1`} />
        <select value={form.priority} onChange={(e) => setForm((p) => ({ ...p, priority: e.target.value }))} className={inputClass}>
          {['Low', 'Medium', 'High', 'Urgent'].map((p) => <option key={p} value={p}>{p}</option>)}
        </select>
        <select value={form.milestoneId} onChange={(e) => setForm((p) => ({ ...p, milestoneId: e.target.value }))} className={inputClass}>
          <option value="">No milestone</option>
          {(milestones || []).map((m) => <option key={m.id} value={m.id}>{m.title}</option>)}
        </select>
        <Button type="submit" variant="secondary" size="sm" disabled={createTask.isPending}>Add</Button>
      </form>
      <div className="space-y-2">
        {(tasksQuery.data || []).map((t) => (
          <div key={t.id} className="flex items-center justify-between rounded-lg border border-border bg-surface px-3 py-2 text-sm">
            <div>
              <p className="font-medium text-heading">{t.title}</p>
              <p className="text-xs text-body/60">{t.priority}{t.milestoneTitle ? ` · ${t.milestoneTitle}` : ''}{t.assignedToName ? ` · ${t.assignedToName}` : ''}</p>
            </div>
            <div className="flex items-center gap-2">
              {t.status !== 'Done' && (
                <button type="button" onClick={() => updateTaskStatus.mutate({ taskId: t.id, payload: { status: 'Done' } })} className="text-xs text-primary hover:underline">Complete</button>
              )}
              <Badge tone={t.status === 'Done' ? 'success' : 'neutral'}>{t.status}</Badge>
              <button type="button" onClick={() => removeTask.mutate(t.id)} className="text-xs text-danger hover:underline">Remove</button>
            </div>
          </div>
        ))}
        {(tasksQuery.data || []).length === 0 && <p className="text-sm text-body/60">No tasks yet.</p>}
      </div>
    </div>
  );
}

function MilestonesTab({ projectId }) {
  const milestonesQuery = useResearchMilestones(projectId);
  const { createMilestone, removeMilestone } = useResearchWorkItemMutations(projectId);
  const [form, setForm] = useState({ title: '', targetDate: '' });

  const handleAdd = (e) => {
    e.preventDefault();
    if (!form.title) return;
    createMilestone.mutate(
      { ...form, targetDate: form.targetDate || null, orderIndex: (milestonesQuery.data || []).length },
      { onSuccess: () => setForm({ title: '', targetDate: '' }) },
    );
  };

  return (
    <div>
      <form onSubmit={handleAdd} className="mb-3 flex flex-wrap gap-2">
        <input placeholder="Milestone title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className={`${inputClass} flex-1`} />
        <input type="date" value={form.targetDate} onChange={(e) => setForm((p) => ({ ...p, targetDate: e.target.value }))} className={inputClass} />
        <Button type="submit" variant="secondary" size="sm" disabled={createMilestone.isPending}>Add</Button>
      </form>
      <div className="space-y-2">
        {(milestonesQuery.data || []).map((m) => (
          <div key={m.id} className="flex items-center justify-between rounded-lg border border-border bg-surface px-3 py-2 text-sm">
            <span>{m.title}{m.targetDate ? ` · ${new Date(m.targetDate).toLocaleDateString()}` : ''} · {m.taskCount} task(s)</span>
            <div className="flex items-center gap-2">
              <Badge tone={m.status === 'Achieved' ? 'success' : 'neutral'}>{m.status}</Badge>
              <button type="button" onClick={() => removeMilestone.mutate(m.id)} className="text-xs text-danger hover:underline">Remove</button>
            </div>
          </div>
        ))}
        {(milestonesQuery.data || []).length === 0 && <p className="text-sm text-body/60">No milestones yet.</p>}
      </div>
    </div>
  );
}

function PublicationsTab({ projectId }) {
  const publicationsQuery = useProjectPublications(projectId);
  const { createPublication, removePublication } = useResearchWorkItemMutations(projectId);
  const [form, setForm] = useState({ title: '', authors: '', type: 'JournalArticle', venue: '', isPublic: false });

  const handleAdd = (e) => {
    e.preventDefault();
    if (!form.title || !form.authors) return;
    createPublication.mutate(form, { onSuccess: () => setForm({ title: '', authors: '', type: 'JournalArticle', venue: '', isPublic: false }) });
  };

  return (
    <div>
      <form onSubmit={handleAdd} className="mb-3 grid gap-2 sm:grid-cols-2">
        <input placeholder="Title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
        <input placeholder="Authors" value={form.authors} onChange={(e) => setForm((p) => ({ ...p, authors: e.target.value }))} className={inputClass} />
        <select value={form.type} onChange={(e) => setForm((p) => ({ ...p, type: e.target.value }))} className={inputClass}>
          {publicationTypes.map((t) => <option key={t} value={t}>{t}</option>)}
        </select>
        <input placeholder="Venue" value={form.venue} onChange={(e) => setForm((p) => ({ ...p, venue: e.target.value }))} className={inputClass} />
        <label className="flex items-center gap-2 text-sm text-body/70">
          <input type="checkbox" checked={form.isPublic} onChange={(e) => setForm((p) => ({ ...p, isPublic: e.target.checked }))} /> Publish to public repository
        </label>
        <Button type="submit" variant="secondary" size="sm" className="sm:col-span-2" disabled={createPublication.isPending}>Add Publication</Button>
      </form>
      <div className="space-y-2">
        {(publicationsQuery.data || []).map((p) => (
          <div key={p.id} className="flex items-center justify-between rounded-lg border border-border bg-surface px-3 py-2 text-sm">
            <div>
              <p className="font-medium text-heading">{p.title}</p>
              <p className="text-xs text-body/60">{p.authors} · {p.type}{p.venue ? ` · ${p.venue}` : ''}{p.isPublic ? ' · Public' : ''}</p>
            </div>
            <button type="button" onClick={() => removePublication.mutate(p.id)} className="text-xs text-danger hover:underline">Remove</button>
          </div>
        ))}
        {(publicationsQuery.data || []).length === 0 && <p className="text-sm text-body/60">No publications yet.</p>}
      </div>
    </div>
  );
}

function MembersTab({ project }) {
  const { addMember, removeMember } = useResearchProjectMutations();
  const [form, setForm] = useState({ userId: '', role: 'Contributor' });

  const handleAdd = (e) => {
    e.preventDefault();
    if (!form.userId) return;
    addMember.mutate({ id: project.id, payload: form }, { onSuccess: () => setForm({ userId: '', role: 'Contributor' }) });
  };

  return (
    <div>
      <div className="mb-3 space-y-2">
        {project.members.map((m) => (
          <div key={m.id} className="flex items-center justify-between rounded-lg border border-border bg-surface px-3 py-2 text-sm">
            <span>{m.userName} ({m.userEmail}) — {m.role}</span>
            {m.userId !== project.ownerUserId && (
              <button type="button" onClick={() => removeMember.mutate({ id: project.id, memberId: m.id })} className="text-xs text-danger hover:underline">Remove</button>
            )}
          </div>
        ))}
      </div>
      <form onSubmit={handleAdd} className="flex flex-wrap gap-2">
        <input placeholder="User ID" value={form.userId} onChange={(e) => setForm((p) => ({ ...p, userId: e.target.value }))} className={`${inputClass} flex-1`} />
        <select value={form.role} onChange={(e) => setForm((p) => ({ ...p, role: e.target.value }))} className={inputClass}>
          {['Contributor', 'CoInvestigator', 'Reviewer'].map((r) => <option key={r} value={r}>{r}</option>)}
        </select>
        <Button type="submit" variant="secondary" size="sm" disabled={addMember.isPending}>Add Member</Button>
      </form>
    </div>
  );
}

function ProjectDetail({ id }) {
  const detailQuery = useResearchProject(id);
  const { updateStatus } = useResearchProjectMutations();
  const [tab, setTab] = useState('Overview');

  const project = detailQuery.data;
  if (detailQuery.isLoading) return <p className="py-4 text-sm text-body/60">Loading…</p>;
  if (!project) return null;

  return (
    <div className="mt-4 border-t border-border pt-4">
      <div className="mb-4 flex flex-wrap items-center justify-between gap-2">
        <div>
          <p className="text-sm font-semibold text-heading">{project.title}</p>
          <p className="text-xs text-body/60">{project.discipline || 'General'} · {project.institution || 'Independent'} · My role: {project.myRole}</p>
        </div>
        <select value={project.status} onChange={(e) => updateStatus.mutate({ id, payload: { status: e.target.value } })} className={inputClass}>
          {['Planning', 'Active', 'Review', 'Completed', 'Archived'].map((s) => <option key={s} value={s}>{s}</option>)}
        </select>
      </div>

      <div className="mb-4 flex flex-wrap gap-2 border-b border-border">
        {tabs.map((t) => (
          <button key={t} type="button" onClick={() => setTab(t)} className={`border-b-2 px-3 py-2 text-sm font-medium ${tab === t ? 'border-primary text-primary' : 'border-transparent text-body/60'}`}>
            {t}
          </button>
        ))}
      </div>

      {tab === 'Overview' && <OverviewTab project={project} />}
      {tab === 'Papers' && <PapersTab projectId={id} />}
      {tab === 'Notes' && <NotesTab projectId={id} />}
      {tab === 'Tasks' && <TasksTab projectId={id} />}
      {tab === 'Milestones' && <MilestonesTab projectId={id} />}
      {tab === 'Publications' && <PublicationsTab projectId={id} />}
      {tab === 'Members' && <MembersTab project={project} />}
    </div>
  );
}

export default function ResearchWorkspace() {
  const { data, isLoading, isError, error } = useResearchProjects({ pageSize: 50 });
  const { create } = useResearchProjectMutations();
  const [showForm, setShowForm] = useState(false);
  const [selectedId, setSelectedId] = useState(null);
  const [form, setForm] = useState({ title: '', summary: '', discipline: '' });

  const projects = data?.items || [];

  const handleCreate = (event) => {
    event.preventDefault();
    create.mutate(form, {
      onSuccess: (result) => { setShowForm(false); setForm({ title: '', summary: '', discipline: '' }); setSelectedId(result.id); },
    });
  };

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Innovation Hub', path: routePaths.research },
          { label: 'Research Workspace' },
        ]}
        title="Research Workspace"
        description="Ongoing and proposed heritage research projects."
        action={<Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'New Research Project'}</Button>}
      />

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <input required placeholder="Title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <textarea required rows={2} placeholder="Summary" value={form.summary} onChange={(e) => setForm((p) => ({ ...p, summary: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <input placeholder="Discipline" value={form.discipline} onChange={(e) => setForm((p) => ({ ...p, discipline: e.target.value }))} className={inputClass} />
          <Button type="submit" variant="primary" disabled={create.isPending}>{create.isPending ? 'Creating…' : 'Create Project'}</Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {projects.map((p) => (
            <div key={p.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{p.title}</p>
                  <p className="text-xs text-body/60">{p.discipline || 'General'} · {p.memberCount} member(s) · {p.openTaskCount} open task(s) · Owner {p.ownerName}</p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={statusTone[p.status] || 'neutral'}>{p.status}</Badge>
                  <Button variant="secondary" onClick={() => setSelectedId(selectedId === p.id ? null : p.id)}>
                    {selectedId === p.id ? 'Hide' : 'Open'}
                  </Button>
                </div>
              </div>
              {selectedId === p.id && <ProjectDetail id={p.id} />}
            </div>
          ))}
          {projects.length === 0 && <p className="text-sm text-body/60">No research projects yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
