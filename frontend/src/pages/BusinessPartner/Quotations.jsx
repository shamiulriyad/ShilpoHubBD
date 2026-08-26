import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useMyQuotations, useQuotationMutations, useQuotation } from '../../hooks/useQuotations';

const statusTone = { Sent: 'secondary', PartiallyResponded: 'primary', Responded: 'success', Closed: 'neutral', Cancelled: 'neutral' };

export default function Quotations() {
  const { data, isLoading, isError, error } = useMyQuotations({ pageSize: 50 });
  const { create, decideResponse, cancel } = useQuotationMutations();
  const [expandedId, setExpandedId] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ title: '', requiredDeliveryDate: '', producerIds: '', productName: '', quantity: '' });

  const requests = data?.items || [];

  const handleCreate = (event) => {
    event.preventDefault();
    create.mutate(
      {
        title: form.title,
        requiredDeliveryDate: form.requiredDeliveryDate,
        producerIds: form.producerIds.split(',').map((s) => s.trim()).filter(Boolean),
        items: [{ productName: form.productName, quantity: Number(form.quantity) }],
      },
      { onSuccess: () => setShowForm(false) },
    );
  };

  return (
    <div>
      <PageHeader
        title="Quotations"
        description="Request quotes from multiple producers and compare their responses."
        action={<Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'New RFQ'}</Button>}
      />

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 space-y-3 rounded-xl border border-border bg-surface p-4">
          <input required placeholder="Title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <input required type="date" value={form.requiredDeliveryDate} onChange={(e) => setForm((p) => ({ ...p, requiredDeliveryDate: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <input required placeholder="Producer IDs (comma separated)" value={form.producerIds} onChange={(e) => setForm((p) => ({ ...p, producerIds: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <div className="grid gap-3 sm:grid-cols-2">
            <input required placeholder="Product name" value={form.productName} onChange={(e) => setForm((p) => ({ ...p, productName: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
            <input required type="number" placeholder="Quantity" value={form.quantity} onChange={(e) => setForm((p) => ({ ...p, quantity: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
          </div>
          <Button type="submit" variant="primary" disabled={create.isPending}>Send RFQ</Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {requests.map((request) => (
            <div key={request.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{request.title}</p>
                  <p className="text-xs text-body/60">{request.referenceNumber} · {request.responseCount}/{request.recipientCount} responded</p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={statusTone[request.status] || 'neutral'}>{request.status}</Badge>
                  <Button variant="secondary" onClick={() => setExpandedId(expandedId === request.id ? null : request.id)}>
                    {expandedId === request.id ? 'Hide' : 'Responses'}
                  </Button>
                </div>
              </div>
              {expandedId === request.id && (
                <QuotationDetailPanel requestId={request.id} onDecide={decideResponse} onCancel={cancel} status={request.status} />
              )}
            </div>
          ))}
          {requests.length === 0 && <p className="text-sm text-body/60">You haven't sent any RFQs yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}

function QuotationDetailPanel({ requestId, onDecide, onCancel, status }) {
  const { data } = useQuotation(requestId);

  return (
    <div className="mt-4 space-y-3 border-t border-border pt-4">
      {(data?.recipients || []).map((r) => (
        <div key={r.id} className="flex flex-wrap items-center justify-between gap-2 rounded-lg border border-border bg-background p-3 text-sm">
          <div>
            <p className="font-medium text-heading">{r.producerName}</p>
            {r.response ? (
              <p className="text-xs text-body/60">৳ {r.response.totalPrice.toLocaleString()}{r.response.notes ? ` · ${r.response.notes}` : ''}</p>
            ) : (
              <p className="text-xs text-body/50">No response yet ({r.status})</p>
            )}
          </div>
          {r.response && r.response.status === 'Submitted' && (
            <div className="flex gap-2">
              <Button variant="primary" onClick={() => onDecide.mutate({ id: requestId, responseId: r.response.id, payload: { status: 'Accepted' } })}>Accept</Button>
              <Button variant="secondary" onClick={() => onDecide.mutate({ id: requestId, responseId: r.response.id, payload: { status: 'Rejected' } })}>Reject</Button>
            </div>
          )}
        </div>
      ))}
      {status !== 'Closed' && status !== 'Cancelled' && (
        <Button variant="secondary" onClick={() => onCancel.mutate(requestId)}>Cancel RFQ</Button>
      )}
    </div>
  );
}
