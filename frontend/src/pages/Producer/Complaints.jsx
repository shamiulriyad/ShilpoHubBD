import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import MutationFeedback from '../../components/ui/MutationFeedback';
import TravelEmptyState from '../../components/ui/TravelEmptyState';
import { resolveUploadUrl } from '../../components/messaging/ImageAttachButton';
import { useReceivedComplaints, useComplaintMutations } from '../../hooks/useOrderComplaints';

const STATUS_LABEL = { Open: 'Needs your answer', Resolved: 'Waiting for the customer', Satisfied: 'Customer satisfied', Withdrawn: 'Withdrawn' };
const STATUS_TONE = { Open: 'secondary', Resolved: 'primary', Satisfied: 'success', Withdrawn: 'neutral' };

function Card({ c, respond }) {
  const [message, setMessage] = useState('');
  return (
    <article className="rounded-xl border border-border bg-surface p-4">
      <div className="flex flex-wrap items-start justify-between gap-2">
        <div>
          <p className="text-sm font-semibold text-heading">{c.subject}</p>
          <p className="text-xs text-body/60">{c.productName} · order {c.orderNumber} · from {c.customerName} · {new Date(c.createdAt).toLocaleDateString()}</p>
        </div>
        <Badge tone={STATUS_TONE[c.status] || 'neutral'}>{STATUS_LABEL[c.status] || c.status}</Badge>
      </div>
      <p className="mt-3 whitespace-pre-line text-sm text-body/80">{c.description}</p>
      {c.imageUrl && <img src={resolveUploadUrl(c.imageUrl)} alt="Photo from the customer" className="mt-2 max-h-48 rounded-lg object-cover" loading="lazy" />}
      {c.customerNote && <p className="mt-2 text-xs text-body/70">Customer note: “{c.customerNote}”</p>}
      {c.producerResponse && <p className="mt-2 rounded-md bg-background px-3 py-2 text-sm text-body/80"><span className="font-medium text-heading">Your answer:</span> {c.producerResponse}</p>}
      {c.status === 'Open' && (
        <form className="mt-4 flex flex-col gap-2 border-t border-border pt-4 sm:flex-row" onSubmit={(e) => { e.preventDefault(); if (message.trim()) respond.mutate({ id: c.id, message: message.trim() }, { onSuccess: () => setMessage('') }); }}>
          <textarea aria-label="Your answer" required rows={2} maxLength={2000} placeholder="Explain what you did to fix it (replacement, refund, repair…)" value={message} onChange={(e) => setMessage(e.target.value)} className="flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <Button type="submit" variant="primary" disabled={respond.isPending || !message.trim()}>Mark resolved</Button>
        </form>
      )}
    </article>
  );
}

export default function ProducerComplaints() {
  const { data, isLoading, isError, error } = useReceivedComplaints();
  const { respond } = useComplaintMutations();
  const complaints = data || [];
  return (
    <div>
      <PageHeader title="Customer Complaints" description="Problems customers reported with your products. Fix them and answer here; the customer then confirms and can rate you." />
      <MutationFeedback mutation={respond} successMessage="Answer sent. The customer has been notified." />
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {complaints.map((c) => <Card key={c.id} c={c} respond={respond} />)}
          {complaints.length === 0 && <TravelEmptyState title="No complaints" description="Nothing reported. Keep up the good work." />}
        </div>
      </AsyncState>
    </div>
  );
}
