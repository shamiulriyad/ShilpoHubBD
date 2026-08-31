import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useReceivedDevelopmentProjects, useProductDevelopmentMutations } from '../../hooks/useProductDevelopment';

const statusTone = { Requested: 'secondary', Active: 'primary', Declined: 'neutral', Approved: 'success', Converted: 'success', Cancelled: 'neutral' };

export default function ProductDevelopment() {
  const { data, isLoading, isError, error } = useReceivedDevelopmentProjects({ pageSize: 50 });
  const { respond, addComment, submitPrototype } = useProductDevelopmentMutations();
  const [expandedId, setExpandedId] = useState(null);
  const [comment, setComment] = useState('');
  const [prototypeDesc, setPrototypeDesc] = useState('');

  const projects = data?.items || [];

  return (
    <div>
      <PageHeader title="Product Development" description="New product development requests from business partners." />
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {projects.map((project) => (
            <div key={project.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{project.title}</p>
                  <p className="text-xs text-body/60">{project.prototypeVersionCount} prototype version(s)</p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={statusTone[project.status] || 'neutral'}>{project.status}</Badge>
                  <Button variant="secondary" onClick={() => setExpandedId(expandedId === project.id ? null : project.id)}>
                    {expandedId === project.id ? 'Hide' : 'Details'}
                  </Button>
                </div>
              </div>

              {expandedId === project.id && (
                <div className="mt-4 space-y-4 border-t border-border pt-4">
                  {project.status === 'Requested' && (
                    <div className="flex gap-2">
                      <Button variant="primary" onClick={() => respond.mutate({ id: project.id, accept: true })}>Accept</Button>
                      <Button variant="secondary" onClick={() => respond.mutate({ id: project.id, accept: false })}>Decline</Button>
                    </div>
                  )}

                  <div className="space-y-2">
                    {(project.comments || []).map((c) => (
                      <p key={c.id} className="text-sm text-body/70"><span className="font-medium text-heading">{c.authorName}:</span> {c.content}</p>
                    ))}
                  </div>
                  <div className="flex gap-2">
                    <input
                      placeholder="Add a comment…"
                      value={comment}
                      onChange={(event) => setComment(event.target.value)}
                      className="flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm"
                    />
                    <Button variant="secondary" onClick={() => { addComment.mutate({ id: project.id, content: comment }); setComment(''); }}>
                      Comment
                    </Button>
                  </div>

                  {project.status === 'Active' && (
                    <div className="flex gap-2">
                      <input
                        placeholder="Describe this prototype…"
                        value={prototypeDesc}
                        onChange={(event) => setPrototypeDesc(event.target.value)}
                        className="flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm"
                      />
                      <Button
                        variant="primary"
                        onClick={() => { submitPrototype.mutate({ id: project.id, payload: { description: prototypeDesc, files: [] } }); setPrototypeDesc(''); }}
                      >
                        Submit Prototype
                      </Button>
                    </div>
                  )}
                </div>
              )}
            </div>
          ))}
          {projects.length === 0 && <p className="text-sm text-body/60">No product development requests yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
