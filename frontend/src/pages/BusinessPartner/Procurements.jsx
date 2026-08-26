import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useMyProcurements, useProcurementMutations } from '../../hooks/useProcurements';

const statusTone = { PendingApproval: 'secondary', Approved: 'primary', Rejected: 'neutral', Converted: 'success', Cancelled: 'neutral' };

export default function Procurements() {
  const { data, isLoading, isError, error } = useMyProcurements({ pageSize: 50 });
  const { create, approve, reject, convertToOrder, cancel } = useProcurementMutations();
  const [expandedId, setExpandedId] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ title: '', producerId: '', deliveryDeadline: '', productId: '', quantity: '', unitPrice: '' });

  const requests = data?.items || [];

  const handleCreate = (event) => {
    event.preventDefault();
    create.mutate(
      {
        title: form.title,
        producerId: form.producerId,
        deliveryDeadline: form.deliveryDeadline,
        items: [{ productId: form.productId, quantity: Number(form.quantity), unitPrice: Number(form.unitPrice) }],
      },
      { onSuccess: () => setShowForm(false) },
    );
  };

  return (
    <div>
      <PageHeader
        title="Procurement Requests"
        description="Internal procurement requests against a chosen producer."
        action={<Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'New Request'}</Button>}
      />

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 space-y-3 rounded-xl border border-border bg-surface p-4">
          <input required placeholder="Title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <input required placeholder="Producer ID" value={form.producerId} onChange={(e) => setForm((p) => ({ ...p, producerId: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <input required type="date" value={form.deliveryDeadline} onChange={(e) => setForm((p) => ({ ...p, deliveryDeadline: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <div className="grid gap-3 sm:grid-cols-3">
            <input required placeholder="Product ID" value={form.productId} onChange={(e) => setForm((p) => ({ ...p, productId: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
            <input required type="number" placeholder="Quantity" value={form.quantity} onChange={(e) => setForm((p) => ({ ...p, quantity: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
            <input required type="number" placeholder="Unit price" value={form.unitPrice} onChange={(e) => setForm((p) => ({ ...p, unitPrice: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
          </div>
          <Button type="submit" variant="primary" disabled={create.isPending}>Create Request</Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {requests.map((req) => (
            <div key={req.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{req.title}</p>
                  <p className="text-xs text-body/60">{req.referenceNumber} · {req.producerName} · ৳ {req.itemsTotal.toLocaleString()}</p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={statusTone[req.status] || 'neutral'}>{req.status}</Badge>
                  <Button variant="secondary" onClick={() => setExpandedId(expandedId === req.id ? null : req.id)}>
                    {expandedId === req.id ? 'Hide' : 'Details'}
                  </Button>
                </div>
              </div>
              {expandedId === req.id && (
                <div className="mt-4 space-y-3 border-t border-border pt-4">
                  <div className="flex flex-wrap gap-2">
                    {req.status === 'PendingApproval' && (
                      <>
                        <Button variant="primary" onClick={() => approve.mutate({ id: req.id })}>Approve</Button>
                        <Button variant="secondary" onClick={() => reject.mutate({ id: req.id })}>Reject</Button>
                      </>
                    )}
                    {req.status === 'Approved' && (
                      <Button variant="primary" onClick={() => convertToOrder.mutate(req.id)}>Convert to Order</Button>
                    )}
                    {!['Converted', 'Cancelled', 'Rejected'].includes(req.status) && (
                      <Button variant="secondary" onClick={() => cancel.mutate(req.id)}>Cancel</Button>
                    )}
                  </div>
                </div>
              )}
            </div>
          ))}
          {requests.length === 0 && <p className="text-sm text-body/60">No procurement requests yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
