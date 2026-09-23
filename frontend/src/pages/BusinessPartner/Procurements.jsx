import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useMyProcurements, useProcurementMutations } from '../../hooks/useProcurements';
import { ProducerSelect, ProducerProductSelect } from '../../components/forms/EntityPickers';

const statusTone = { PendingApproval: 'secondary', Approved: 'primary', Rejected: 'neutral', Converted: 'success', Cancelled: 'neutral' };

export default function Procurements() {
  const { data, isLoading, isError, error } = useMyProcurements({ pageSize: 50 });
  const { create, approve, reject, convertToOrder, cancel } = useProcurementMutations();
  const [expandedId, setExpandedId] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ title: '', producerId: '', deliveryDeadline: '', productId: '', quantity: '', unitPrice: '' });

  const requests = data?.items || [];

  const today = new Date().toISOString().slice(0, 10);
  const quantityOk = Number(form.quantity) >= 1 && Number.isInteger(Number(form.quantity));
  const priceOk = Number(form.unitPrice) > 0;
  const formValid = Boolean(form.title.trim() && form.producerId && form.productId && form.deliveryDeadline && quantityOk && priceOk);

  const handleCreate = (event) => {
    event.preventDefault();
    if (!formValid) return;
    create.mutate(
      {
        title: form.title.trim(),
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
        <form onSubmit={handleCreate} className="mb-6 grid gap-4 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <label className="block text-sm sm:col-span-2">
            <span className="mb-1 block font-medium text-heading">Title</span>
            <input required placeholder="e.g. Jamdani sarees for autumn collection" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          </label>
          <ProducerSelect id="proc-producer" label="Producer" required value={form.producerId} onChange={(v) => setForm((p) => ({ ...p, producerId: v, productId: '' }))} />
          <label className="block text-sm">
            <span className="mb-1 block font-medium text-heading">Delivery deadline</span>
            <input required type="date" min={today} value={form.deliveryDeadline} onChange={(e) => setForm((p) => ({ ...p, deliveryDeadline: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          </label>
          <div className="sm:col-span-2">
            <ProducerProductSelect id="proc-product" label="Product" required producerId={form.producerId} value={form.productId} onChange={(v) => setForm((p) => ({ ...p, productId: v }))} />
          </div>
          <label className="block text-sm">
            <span className="mb-1 block font-medium text-heading">Quantity</span>
            <input required type="number" min="1" step="1" placeholder="Units" value={form.quantity} onChange={(e) => setForm((p) => ({ ...p, quantity: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          </label>
          <label className="block text-sm">
            <span className="mb-1 block font-medium text-heading">Unit price (৳)</span>
            <input required type="number" min="1" step="any" placeholder="Price per unit" value={form.unitPrice} onChange={(e) => setForm((p) => ({ ...p, unitPrice: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          </label>
          {quantityOk && priceOk && (
            <p className="text-sm text-body/70 sm:col-span-2">Estimated total: <span className="font-semibold text-heading">৳ {(Number(form.quantity) * Number(form.unitPrice)).toLocaleString('en-BD')}</span></p>
          )}
          <div className="sm:col-span-2">
            <Button type="submit" variant="primary" disabled={create.isPending || !formValid}>{create.isPending ? 'Creating…' : 'Create Request'}</Button>
          </div>
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
