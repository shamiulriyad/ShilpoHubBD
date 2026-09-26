import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import MutationFeedback from '../../components/ui/MutationFeedback';
import TravelEmptyState from '../../components/ui/TravelEmptyState';
import ProcurementAdvanceInfo from '../../components/business/ProcurementAdvanceInfo';
import { useIncomingProcurements, useProcurementMutations } from '../../hooks/useProcurements';

const statusTone = { PendingApproval: 'secondary', Approved: 'primary', Rejected: 'neutral', Converted: 'success', Cancelled: 'neutral' };
const statusLabel = { PendingApproval: 'Needs your answer', Approved: 'You accepted', Rejected: 'Declined', Converted: 'Order placed', Cancelled: 'Cancelled by the partner' };

export default function ProducerProcurements() {
  const { data, isLoading, isError, error } = useIncomingProcurements({ pageSize: 50 });
  const { approve, reject } = useProcurementMutations();
  const [notes, setNotes] = useState({});
  const requests = data?.items || [];

  return (
    <div>
      <PageHeader
        title="Bulk Procurement Requests"
        description="Business partners who want to buy from you in bulk. Accept, then they pay at least 50% in advance and an admin inspects the deal."
      />
      <MutationFeedback mutation={approve} successMessage="Accepted. The partner can now pay the advance." />
      <MutationFeedback mutation={reject} successMessage="Declined. The partner has been told." />
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {requests.map((req) => (
            <article key={req.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{req.title}</p>
                  <p className="text-xs text-body/60">{req.referenceNumber} · from {req.businessPartnerName} · needed by {new Date(req.deliveryDeadline).toLocaleDateString()}</p>
                </div>
                <Badge tone={statusTone[req.status] || 'neutral'}>{statusLabel[req.status] || req.status}</Badge>
              </div>
              <div className="mt-3"><ProcurementAdvanceInfo req={req} /></div>
              {req.status === 'PendingApproval' && (
                <div className="mt-4 flex flex-wrap items-center gap-2 border-t border-border pt-4">
                  <input aria-label="Note to the partner" placeholder="Note to the partner (optional)" value={notes[req.id] || ''} onChange={(e) => setNotes((p) => ({ ...p, [req.id]: e.target.value }))} className="min-w-[14rem] flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm" />
                  <Button variant="primary" disabled={approve.isPending} onClick={() => approve.mutate({ id: req.id, notes: notes[req.id]?.trim() || undefined })}>Accept</Button>
                  <Button variant="secondary" disabled={reject.isPending} onClick={() => reject.mutate({ id: req.id, notes: notes[req.id]?.trim() || undefined })}>Decline</Button>
                </div>
              )}
            </article>
          ))}
          {requests.length === 0 && <TravelEmptyState title="No procurement requests" description="When a business partner sends you a bulk request, it appears here." />}
        </div>
      </AsyncState>
    </div>
  );
}
