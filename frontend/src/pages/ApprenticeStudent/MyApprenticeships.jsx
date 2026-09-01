import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import {
  useMyApprenticeEnrollments, useApprenticeEnrollment, useApprenticeEnrollmentMutations,
} from '../../hooks/useApprenticeEnrollments';

const statusTone = { Active: 'primary', Completed: 'success', Withdrawn: 'neutral' };

function EnrollmentMilestones({ id }) {
  const detailQuery = useApprenticeEnrollment(id);
  const { updateMilestoneProgress, complete } = useApprenticeEnrollmentMutations();

  const enrollment = detailQuery.data;
  if (detailQuery.isLoading) return <p className="py-2 text-xs text-body/60">Loading milestones…</p>;
  if (!enrollment) return null;

  return (
    <div className="mt-3 space-y-2 border-t border-border pt-3">
      {enrollment.milestones.map((m) => (
        <div key={m.milestoneId} className={`flex items-center justify-between rounded-lg border px-3 py-2 text-xs ${m.isCompleted ? 'border-success/30 bg-success/5' : 'border-border'}`}>
          <span>{m.title}{m.notes ? ` — ${m.notes}` : ''}</span>
          {m.isCompleted ? (
            <Badge tone="success">Done</Badge>
          ) : (
            <button
              type="button"
              onClick={() => updateMilestoneProgress.mutate({ id, milestoneId: m.milestoneId, payload: { isCompleted: true } })}
              className="text-primary hover:underline"
            >
              Mark complete
            </button>
          )}
        </div>
      ))}
      {enrollment.milestones.length === 0 && <p className="text-xs text-body/50">No milestones defined for this program yet.</p>}

      {enrollment.status === 'Active' && enrollment.progressPercent >= 100 && (
        <Button size="sm" variant="primary" disabled={complete.isPending} onClick={() => complete.mutate(id)}>
          Complete Apprenticeship
        </Button>
      )}
    </div>
  );
}

export default function MyApprenticeships() {
  const { data, isLoading, isError, error } = useMyApprenticeEnrollments();
  const [expandedId, setExpandedId] = useState(null);

  const enrollments = data || [];

  return (
    <div>
      <PageHeader title="My Apprenticeships" description="Track your progress through enrolled apprenticeship and workshop programs." />

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-4">
          {enrollments.map((e) => (
            <div key={e.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
                <p className="text-sm font-semibold text-heading">{e.programTitle}</p>
                <div className="flex items-center gap-2">
                  <Badge tone={statusTone[e.status] || 'neutral'}>{e.status}</Badge>
                  <Button variant="secondary" onClick={() => setExpandedId(expandedId === e.id ? null : e.id)}>
                    {expandedId === e.id ? 'Hide' : 'Milestones'}
                  </Button>
                </div>
              </div>

              <div className="mb-1 h-2 w-full overflow-hidden rounded-full bg-background">
                <div className="h-full rounded-full bg-primary" style={{ width: `${e.progressPercent}%` }} />
              </div>
              <p className="text-xs text-body/60">{e.progressPercent}% complete</p>

              {expandedId === e.id && <EnrollmentMilestones id={e.id} />}
            </div>
          ))}
          {enrollments.length === 0 && <p className="text-sm text-body/60">You're not enrolled in any apprenticeship programs yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
