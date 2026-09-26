import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import MutationFeedback from '../../components/ui/MutationFeedback';
import TravelEmptyState from '../../components/ui/TravelEmptyState';
import { useProfileApprovals, useProfileReviewMutations } from '../../hooks/useProfile';

const STATUS_TONE = { Pending: 'secondary', Approved: 'success', Rejected: 'neutral' };

export default function ProfileApprovals() {
  const [status, setStatus] = useState('Pending');
  const [search, setSearch] = useState('');
  const { data, isLoading, isError, error } = useProfileApprovals({ status: status || undefined, search: search || undefined, pageSize: 50 });
  const { approve, reject } = useProfileReviewMutations();
  const [notes, setNotes] = useState({});
  const profiles = data?.items || [];

  return (
    <div>
      <PageHeader
        title="Profile Approvals"
        description="Check each member's NID and details, then approve or send it back with a reason."
      />
      <div className="mb-4 flex flex-wrap gap-2">
        <select aria-label="Status" value={status} onChange={(e) => setStatus(e.target.value)} className="rounded-md border border-border bg-background px-3 py-2 text-sm">
          <option value="Pending">Waiting for review</option>
          <option value="Approved">Approved</option>
          <option value="Rejected">Rejected</option>
          <option value="">All</option>
        </select>
        <input aria-label="Search" placeholder="Name, NID or email" value={search} onChange={(e) => setSearch(e.target.value)} className="min-w-[14rem] flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm" />
      </div>
      <MutationFeedback mutation={approve} successMessage="Profile approved. The member has been notified." />
      <MutationFeedback mutation={reject} successMessage="Profile sent back. The member has been notified." />
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {profiles.map((p) => (
            <article key={p.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-start justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{p.legalName} <span className="font-normal text-body/60">({p.roles.join(', ') || 'no role'})</span></p>
                  <p className="text-xs text-body/60">Login: {p.loginName} · {p.loginEmail}</p>
                </div>
                <Badge tone={STATUS_TONE[p.status] || 'neutral'}>{p.status}</Badge>
              </div>
              <dl className="mt-3 grid grid-cols-2 gap-3 text-xs sm:grid-cols-4">
                <div><dt className="text-body/60">NID</dt><dd className="font-semibold text-heading">{p.nidNumber}</dd></div>
                <div><dt className="text-body/60">Phone</dt><dd className="font-semibold text-heading">{p.phone}</dd></div>
                <div><dt className="text-body/60">Expertise</dt><dd className="font-semibold text-heading">{p.expertise || '—'}</dd></div>
                <div><dt className="text-body/60">Location</dt><dd className="font-semibold text-heading">{[p.districtName, p.addressLine].filter(Boolean).join(', ')}</dd></div>
              </dl>
              {p.reviewNotes && <p className="mt-2 text-xs text-body/70">Note: {p.reviewNotes}</p>}
              {p.status === 'Pending' && (
                <div className="mt-4 flex flex-wrap items-center gap-2 border-t border-border pt-4">
                  <input aria-label="Note" placeholder="Note (required to reject)" value={notes[p.id] || ''} onChange={(e) => setNotes((n) => ({ ...n, [p.id]: e.target.value }))} className="min-w-[14rem] flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm" />
                  <Button variant="primary" disabled={approve.isPending} onClick={() => approve.mutate({ id: p.id, notes: notes[p.id]?.trim() || undefined })}>Approve</Button>
                  <Button variant="secondary" disabled={reject.isPending || !notes[p.id]?.trim()} onClick={() => reject.mutate({ id: p.id, notes: notes[p.id].trim() })}>Send back</Button>
                </div>
              )}
            </article>
          ))}
          {profiles.length === 0 && <TravelEmptyState title="Nothing here" description="No profiles match this filter." />}
        </div>
      </AsyncState>
    </div>
  );
}
