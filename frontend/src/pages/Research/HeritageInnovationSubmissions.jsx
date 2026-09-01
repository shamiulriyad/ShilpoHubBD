import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import {
  useHeritageInnovationSubmissions, useHeritageInnovationSubmission, useHeritageInnovationSubmissionMutations,
} from '../../hooks/useHeritageInnovationSubmissions';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const statusTone = { Draft: 'neutral', Submitted: 'secondary', UnderReview: 'primary', Approved: 'success', Rejected: 'neutral', Withdrawn: 'neutral' };
const decisions = ['Approve', 'Reject', 'RequestChanges'];

const emptyForm = { title: '', problem: '', solution: '', researchEvidence: '' };

function SubmissionDetail({ id }) {
  const detailQuery = useHeritageInnovationSubmission(id);
  const { submit, withdraw, addTeamMember, removeTeamMember, addReview } = useHeritageInnovationSubmissionMutations();
  const [memberId, setMemberId] = useState('');
  const [review, setReview] = useState({ decision: 'Approve', comments: '' });

  const submission = detailQuery.data;
  if (detailQuery.isLoading) return <p className="py-4 text-sm text-body/60">Loading…</p>;
  if (!submission) return null;

  return (
    <div className="mt-4 space-y-4 border-t border-border pt-4">
      <div className="text-xs text-body/60">
        <p><strong>Problem:</strong> {submission.problem}</p>
        <p className="mt-1"><strong>Solution:</strong> {submission.solution}</p>
      </div>

      <div className="flex flex-wrap gap-2">
        {submission.status === 'Draft' && (
          <Button size="sm" variant="primary" disabled={submit.isPending} onClick={() => submit.mutate(id)}>Submit for Review</Button>
        )}
        {['Submitted', 'UnderReview'].includes(submission.status) && (
          <Button size="sm" variant="secondary" disabled={withdraw.isPending} onClick={() => withdraw.mutate(id)}>Withdraw</Button>
        )}
      </div>

      <div>
        <h4 className="mb-2 text-sm font-semibold text-heading">Team ({submission.teamMembers.length})</h4>
        <div className="mb-2 space-y-1">
          {submission.teamMembers.map((m) => (
            <div key={m.id} className="flex items-center justify-between rounded-lg border border-border px-3 py-2 text-xs">
              <span>{m.userName} {m.roleOnTeam ? `(${m.roleOnTeam})` : ''}</span>
              <button type="button" onClick={() => removeTeamMember.mutate({ id, memberId: m.id })} className="text-danger hover:underline">Remove</button>
            </div>
          ))}
          {submission.teamMembers.length === 0 && <p className="text-xs text-body/50">No team members yet.</p>}
        </div>
        <div className="flex gap-2">
          <input placeholder="User ID" value={memberId} onChange={(e) => setMemberId(e.target.value)} className={`${inputClass} flex-1`} />
          <Button
            size="sm"
            variant="secondary"
            disabled={!memberId || addTeamMember.isPending}
            onClick={() => addTeamMember.mutate({ id, payload: { userId: memberId } }, { onSuccess: () => setMemberId('') })}
          >
            Add
          </Button>
        </div>
      </div>

      {submission.canReview && (
        <div>
          <h4 className="mb-2 text-sm font-semibold text-heading">Reviews ({submission.reviews.length})</h4>
          <div className="mb-2 space-y-1 text-xs text-body/60">
            {submission.reviews.map((r) => (
              <p key={r.id}>{r.reviewerName}: {r.decision}{r.score != null ? ` (${r.score}/10)` : ''} — {r.comments}</p>
            ))}
          </div>
          <div className="flex flex-wrap gap-2">
            <select value={review.decision} onChange={(e) => setReview((p) => ({ ...p, decision: e.target.value }))} className={inputClass}>
              {decisions.map((d) => <option key={d} value={d}>{d}</option>)}
            </select>
            <input placeholder="Comments" value={review.comments} onChange={(e) => setReview((p) => ({ ...p, comments: e.target.value }))} className={`${inputClass} flex-1`} />
            <Button size="sm" variant="secondary" disabled={addReview.isPending} onClick={() => addReview.mutate({ id, payload: review })}>
              Submit Review
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}

export default function HeritageInnovationSubmissions() {
  const { data, isLoading, isError, error } = useHeritageInnovationSubmissions({ pageSize: 50 });
  const { create } = useHeritageInnovationSubmissionMutations();
  const [showForm, setShowForm] = useState(false);
  const [expandedId, setExpandedId] = useState(null);
  const [form, setForm] = useState(emptyForm);

  const submissions = data?.items || [];

  const handleCreate = (event) => {
    event.preventDefault();
    create.mutate(form, { onSuccess: () => { setShowForm(false); setForm(emptyForm); } });
  };

  return (
    <div>
      <PageHeader
        title="Heritage Innovation Submissions"
        description="Submit heritage innovation ideas for review, assemble a team, and track reviewer decisions."
        action={<Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'New Submission'}</Button>}
      />

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <input required placeholder="Title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <textarea required rows={2} placeholder="Problem" value={form.problem} onChange={(e) => setForm((p) => ({ ...p, problem: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <textarea required rows={2} placeholder="Solution" value={form.solution} onChange={(e) => setForm((p) => ({ ...p, solution: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <textarea rows={2} placeholder="Research evidence (optional)" value={form.researchEvidence} onChange={(e) => setForm((p) => ({ ...p, researchEvidence: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={create.isPending}>{create.isPending ? 'Creating…' : 'Create Submission'}</Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {submissions.map((s) => (
            <div key={s.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{s.title}</p>
                  <p className="text-xs text-body/60">{s.teamMemberCount} team member(s) · {s.reviewCount} review(s)</p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={statusTone[s.status] || 'neutral'}>{s.status}</Badge>
                  <Button variant="secondary" onClick={() => setExpandedId(expandedId === s.id ? null : s.id)}>
                    {expandedId === s.id ? 'Hide' : 'Manage'}
                  </Button>
                </div>
              </div>
              {expandedId === s.id && <SubmissionDetail id={s.id} />}
            </div>
          ))}
          {submissions.length === 0 && <p className="text-sm text-body/60">No submissions yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
