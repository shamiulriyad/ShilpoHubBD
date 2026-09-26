import { useState } from 'react';
import { Link } from 'react-router-dom';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import MutationFeedback from '../../components/ui/MutationFeedback';
import TravelEmptyState from '../../components/ui/TravelEmptyState';
import { resolveUploadUrl } from '../../components/messaging/ImageAttachButton';
import { routePaths } from '../../routes/routePaths';
import { useMyComplaints, useComplaintMutations } from '../../hooks/useOrderComplaints';

const STATUS_LABEL = { Open: 'Waiting for the producer', Resolved: 'Producer says it is resolved', Satisfied: 'You are satisfied', Withdrawn: 'Withdrawn' };
const STATUS_TONE = { Open: 'secondary', Resolved: 'primary', Satisfied: 'success', Withdrawn: 'neutral' };

function ComplaintCard({ c, m }) {
  const [note, setNote] = useState('');
  return (
    <article className="rounded-xl border border-border bg-surface p-4">
      <div className="flex flex-wrap items-start justify-between gap-2">
        <div>
          <p className="text-sm font-semibold text-heading">{c.subject}</p>
          <p className="text-xs text-body/60">{c.productName} · order {c.orderNumber} · {c.producerName} · {new Date(c.createdAt).toLocaleDateString()}</p>
        </div>
        <Badge tone={STATUS_TONE[c.status] || 'neutral'}>{STATUS_LABEL[c.status] || c.status}</Badge>
      </div>
      <p className="mt-3 whitespace-pre-line text-sm text-body/80">{c.description}</p>
      {c.imageUrl && <img src={resolveUploadUrl(c.imageUrl)} alt="Attached to the complaint" className="mt-2 max-h-48 rounded-lg object-cover" loading="lazy" />}
      {c.producerResponse && <p className="mt-3 rounded-md bg-background px-3 py-2 text-sm text-body/80"><span className="font-medium text-heading">Producer:</span> {c.producerResponse}</p>}

      {c.status === 'Resolved' && (
        <div className="mt-4 space-y-2 border-t border-border pt-4">
          <input aria-label="Note" placeholder="Note (optional)" value={note} onChange={(e) => setNote(e.target.value)} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <div className="flex flex-wrap gap-2">
            <Button variant="primary" disabled={m.satisfied.isPending} onClick={() => m.satisfied.mutate({ id: c.id, note: note.trim() || undefined })}>I am satisfied</Button>
            <Button variant="secondary" disabled={m.reopen.isPending} onClick={() => m.reopen.mutate({ id: c.id, note: note.trim() || undefined })}>Not fixed - reopen</Button>
          </div>
        </div>
      )}
      {c.status === 'Open' && (
        <div className="mt-4 border-t border-border pt-4">
          <Button variant="secondary" disabled={m.withdraw.isPending} onClick={() => m.withdraw.mutate(c.id)}>Withdraw complaint</Button>
        </div>
      )}
      {c.canRate && (
        <div className="mt-4 border-t border-border pt-4">
          <Link to={routePaths.marketplaceProductDetails.replace(':productId', c.productId)} className="text-sm font-semibold text-primary hover:underline">
            Rate the producer and product →
          </Link>
        </div>
      )}
    </article>
  );
}

export default function CustomerComplaints() {
  const { data, isLoading, isError, error } = useMyComplaints();
  const m = useComplaintMutations();
  const complaints = data || [];
  return (
    <div>
      <PageHeader
        title="My Complaints"
        description="Problems you reported with delivered items. Once the producer fixes it and you are satisfied, you can rate the producer and the product."
      />
      {['satisfied', 'reopen', 'withdraw'].map((k) => <MutationFeedback key={k} mutation={m[k]} successMessage="Updated." />)}
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {complaints.map((c) => <ComplaintCard key={c.id} c={c} m={m} />)}
          {complaints.length === 0 && <TravelEmptyState title="No complaints" description="If something arrives wrong, report it from the order in Order History." />}
        </div>
      </AsyncState>
    </div>
  );
}
