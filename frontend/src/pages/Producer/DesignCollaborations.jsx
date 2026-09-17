import { useState } from 'react';
import { Pagination, PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useDesignCollaboration, useReceivedDesignCollaborations, useDesignCollaborationMutations } from '../../hooks/useDesignCollaborations';

import MutationFeedback from '../../components/ui/MutationFeedback';

const statusTone = { Invited: 'secondary', Active: 'primary', Declined: 'neutral', Completed: 'success', Cancelled: 'neutral' };

export default function DesignCollaborations() {
  const [page, setPage] = useState(1);
  const { data, isLoading, isError, error } = useReceivedDesignCollaborations({ page, pageSize: 10 });
  const { respond, addComment, submitRevision } = useDesignCollaborationMutations();
  const [expandedId, setExpandedId] = useState(null);
  const detailQuery = useDesignCollaboration(expandedId);
  const [comment, setComment] = useState('');
  const [revision, setRevision] = useState('');

  const projects = data?.items || [];

  return (
    <div>
      <PageHeader title="Design Collaborations" description="Design projects requested by business partners." />
<div className="mb-4 space-y-2"><MutationFeedback mutation={respond} successMessage="Changes saved." /><MutationFeedback mutation={addComment} successMessage="Changes saved." /><MutationFeedback mutation={submitRevision} successMessage="Changes saved." /></div>
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {projects.map((summary) => {
            const project = expandedId === summary.id && detailQuery.data ? detailQuery.data : summary;
            return (
            <div key={project.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{project.title}</p>
                  <p className="text-xs text-body/60">{project.revisions?.length ?? project.revisionCount} revision{(project.revisions?.length ?? project.revisionCount) === 1 ? '' : 's'}</p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={statusTone[project.status] || 'neutral'}>{project.status}</Badge>
                  <Button variant="secondary" onClick={() => setExpandedId(expandedId === project.id ? null : project.id)}>
                    {expandedId === project.id ? 'Hide' : 'Details'}
                  </Button>
                </div>
              </div>

              {expandedId === project.id && (
                <AsyncState isLoading={detailQuery.isLoading} isError={detailQuery.isError} error={detailQuery.error}>
                <div className="mt-4 space-y-4 border-t border-border pt-4">
                  <p className="text-sm leading-6 text-muted">{project.designRequirements}</p>
                  {project.status === 'Invited' && (
                    <div className="flex flex-wrap gap-2">
                      <Button variant="primary" disabled={respond.isPending} onClick={() => respond.mutate({ id: project.id, accept: true })}>Accept</Button>
                      <Button variant="secondary" disabled={respond.isPending} onClick={() => respond.mutate({ id: project.id, accept: false })}>Decline</Button>
                    </div>
                  )}

                  <div className="space-y-2">
                    {(project.comments || []).map((c) => (
                      <p key={c.id} className="text-sm text-body/70"><span className="font-medium text-heading">{c.authorName}:</span> {c.content}</p>
                    ))}
                  </div>
                  <div className="flex flex-wrap gap-2">
                    <input aria-label="Add a comment"
                      placeholder="Add a comment…"
                      value={comment}
                      onChange={(event) => setComment(event.target.value)}
                      className="min-w-0 flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm"
                    />
                    <Button variant="secondary" disabled={!comment.trim() || addComment.isPending} onClick={() => addComment.mutate({ id: project.id, content: comment.trim() }, { onSuccess: () => setComment('') })}>
                      Comment
                    </Button>
                  </div>

                  {project.status === 'Active' && (
                    <div className="flex flex-wrap gap-2">
                      <input aria-label="Describe this revision"
                        placeholder="Describe this revision…"
                        value={revision}
                        onChange={(event) => setRevision(event.target.value)}
                        className="min-w-0 flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm"
                      />
                      <Button
                        variant="primary"
                        disabled={!revision.trim() || submitRevision.isPending} onClick={() => submitRevision.mutate({ id: project.id, payload: { description: revision.trim(), files: [] } }, { onSuccess: () => setRevision('') })}
                      >
                        Submit Revision
                      </Button>
                    </div>
                  )}
                </div>
                </AsyncState>
              )}
            </div>
          ); })}
          {projects.length === 0 && <p className="text-sm text-body/60">No design collaboration requests yet.</p>}
        </div>
        {data?.totalPages > 1 && <div className="mt-6"><Pagination currentPage={page} totalPages={data.totalPages} onPageChange={setPage} /></div>}
      </AsyncState>
    </div>
  );
}
