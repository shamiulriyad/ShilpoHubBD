import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useMyDesignCollaborations, useDesignCollaborationMutations, useDesignCollaboration } from '../../hooks/useDesignCollaborations';

const statusTone = { Invited: 'secondary', Active: 'primary', Declined: 'neutral', Completed: 'success', Cancelled: 'neutral' };

function ProjectPanel({ id }) {
  const { data: project } = useDesignCollaboration(id);
  const { addComment, decideRevision, complete, cancel } = useDesignCollaborationMutations();
  const [comment, setComment] = useState('');

  if (!project) return null;

  return (
    <div className="mt-4 space-y-3 border-t border-border pt-4">
      <div className="space-y-2">
        {(project.comments || []).map((c) => (
          <p key={c.id} className="text-sm text-body/70"><span className="font-medium text-heading">{c.authorName}:</span> {c.content}</p>
        ))}
      </div>
      <div className="flex gap-2">
        <input placeholder="Add a comment…" value={comment} onChange={(e) => setComment(e.target.value)} className="flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm" />
        <Button variant="secondary" onClick={() => { addComment.mutate({ id: project.id, content: comment }); setComment(''); }}>Comment</Button>
      </div>

      {(project.revisions || []).filter((r) => r.status === 'Pending').map((r) => (
        <div key={r.id} className="flex items-center justify-between rounded-lg border border-border bg-background p-3 text-sm">
          <span>Revision {r.revisionNumber}: {r.description}</span>
          <div className="flex gap-2">
            <Button variant="primary" onClick={() => decideRevision.mutate({ id: project.id, revisionId: r.id, payload: { status: 'Approved' } })}>Approve</Button>
            <Button variant="secondary" onClick={() => decideRevision.mutate({ id: project.id, revisionId: r.id, payload: { status: 'Rejected' } })}>Reject</Button>
          </div>
        </div>
      ))}

      <div className="flex gap-2">
        {project.status === 'Active' && <Button variant="primary" onClick={() => complete.mutate(project.id)}>Mark Complete</Button>}
        {!['Completed', 'Cancelled', 'Declined'].includes(project.status) && <Button variant="secondary" onClick={() => cancel.mutate(project.id)}>Cancel</Button>}
      </div>
    </div>
  );
}

export default function DesignCollaborations() {
  const { data, isLoading, isError, error } = useMyDesignCollaborations({ pageSize: 50 });
  const { create } = useDesignCollaborationMutations();
  const [expandedId, setExpandedId] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ producerId: '', title: '', designRequirements: '' });

  const projects = data?.items || [];

  const handleCreate = (event) => {
    event.preventDefault();
    create.mutate(
      { producerId: form.producerId, title: form.title, designRequirements: form.designRequirements, initialFiles: [] },
      { onSuccess: () => setShowForm(false) },
    );
  };

  return (
    <div>
      <PageHeader
        title="Design Collaborations"
        description="Invite producers to collaborate on new designs."
        action={<Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'New Project'}</Button>}
      />

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 space-y-3 rounded-xl border border-border bg-surface p-4">
          <input required placeholder="Producer ID" value={form.producerId} onChange={(e) => setForm((p) => ({ ...p, producerId: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <input required placeholder="Title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <textarea required rows={3} placeholder="Design requirements" value={form.designRequirements} onChange={(e) => setForm((p) => ({ ...p, designRequirements: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <Button type="submit" variant="primary" disabled={create.isPending}>Send Invite</Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {projects.map((project) => (
            <div key={project.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{project.title}</p>
                  <p className="text-xs text-body/60">{project.producerName} · {project.revisionCount} revisions</p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={statusTone[project.status] || 'neutral'}>{project.status}</Badge>
                  <Button variant="secondary" onClick={() => setExpandedId(expandedId === project.id ? null : project.id)}>
                    {expandedId === project.id ? 'Hide' : 'Details'}
                  </Button>
                </div>
              </div>
              {expandedId === project.id && <ProjectPanel id={project.id} />}
            </div>
          ))}
          {projects.length === 0 && <p className="text-sm text-body/60">You haven't started any design collaborations yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
