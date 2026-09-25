import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import MutationFeedback from '../../components/ui/MutationFeedback';
import TravelEmptyState from '../../components/ui/TravelEmptyState';
import ProcurementAdvanceInfo from '../../components/business/ProcurementAdvanceInfo';
import { useProcurementInspections, useProcurementMutations } from '../../hooks/useProcurements';

export default function ProcurementInspections() {
  const [pendingOnly, setPendingOnly] = useState(true);
  const { data, isLoading, isError, error } = useProcurementInspections({ pendingOnly, pageSize: 50 });
  const { inspect } = useProcurementMutations();
  const [notes, setNotes] = useState({});
  const deals = data?.items || [];

  return (
    <div>
      <PageHeader
        title="Bulk Deal Inspections"
        description="Deals whose business partner has paid the 50% advance. Approve to let the partner place the order; reject to refund the advance."
        action={
          <label className="flex items-center gap-2 text-sm text-body/70">
            <input type="checkbox" checked={pendingOnly} onChange={(e) => setPendingOnly(e.target.checked)} />
            Only waiting for inspection
          </label>
        }
      />
      <MutationFeedback mutation={inspect} successMessage="Inspection recorded. Both parties have been notified." />
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {deals.map((d) => (
            <article key={d.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{d.title}</p>
                  <p className="text-xs text-body/60">{d.referenceNumber} · {d.businessPartnerName} → {d.producerName}</p>
                </div>
                <Badge tone="secondary">{d.status}</Badge>
              </div>
              <div className="mt-3"><ProcurementAdvanceInfo req={d} /></div>
              {d.inspectionStatus === 'Pending' && (
                <div className="mt-4 flex flex-wrap items-center gap-2 border-t border-border pt-4">
                  <input aria-label="Inspection note" placeholder="Inspection note" value={notes[d.id] || ''} onChange={(e) => setNotes((p) => ({ ...p, [d.id]: e.target.value }))} className="min-w-[14rem] flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm" />
                  <Button variant="primary" disabled={inspect.isPending} onClick={() => inspect.mutate({ id: d.id, payload: { approve: true, notes: notes[d.id]?.trim() || undefined } })}>Approve</Button>
                  <Button variant="secondary" disabled={inspect.isPending} onClick={() => inspect.mutate({ id: d.id, payload: { approve: false, notes: notes[d.id]?.trim() || undefined } })}>Reject and refund</Button>
                </div>
              )}
            </article>
          ))}
          {deals.length === 0 && <TravelEmptyState title="Nothing to inspect" description="Deals appear here once a business partner pays the advance." />}
        </div>
      </AsyncState>
    </div>
  );
}
