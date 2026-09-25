import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState, StatusTimeline } from '../../components/ui';
import { ProducerSelect, ProducerProductSelect } from '../../components/forms/EntityPickers';
import { useMyContracts, useContractMutations } from '../../hooks/useContracts';

import { confirmAction } from '../../lib/confirm';
const statusTone = { PendingApproval: 'secondary', Active: 'success', Rejected: 'neutral', Terminated: 'neutral', Expired: 'neutral' };

export default function Contracts() {
  const { data, isLoading, isError, error } = useMyContracts({ pageSize: 50 });
  const { create, terminate } = useContractMutations();
  const [expandedId, setExpandedId] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ producerId: '', title: '', terms: '', startDate: '', endDate: '', productId: '', quantity: '', unitPrice: '' });

  const contracts = data?.items || [];

  const today = new Date().toISOString().slice(0, 10);
  const formValid = Boolean(form.producerId && form.productId && form.title.trim() && form.terms.trim() && form.startDate && form.endDate && form.endDate > form.startDate && Number(form.quantity) >= 1 && Number(form.unitPrice) >= 0 && form.unitPrice !== '');

  const handleCreate = (event) => {
    event.preventDefault();
    if (!formValid) return;
    create.mutate(
      {
        producerId: form.producerId,
        title: form.title,
        terms: form.terms,
        startDate: form.startDate,
        endDate: form.endDate,
        autoRenew: false,
        items: [{ productId: form.productId, quantity: Number(form.quantity), unitPrice: Number(form.unitPrice) }],
        deliverySchedules: [],
      },
      { onSuccess: () => setShowForm(false) },
    );
  };

  return (
    <div>
      <PageHeader
        title="Contracts"
        description="Supply contracts you've offered to producers."
        action={<Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'New Contract'}</Button>}
      />

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 space-y-3 rounded-xl border border-border bg-surface p-4">
          <ProducerSelect id="contract-producer" label="Producer" required value={form.producerId} onChange={(v) => setForm((p) => ({ ...p, producerId: v, productId: '' }))} />
          <ProducerProductSelect id="contract-product" label="Product" required producerId={form.producerId} value={form.productId} onChange={(v) => setForm((p) => ({ ...p, productId: v }))} />
          <div className="grid gap-3 sm:grid-cols-2">
            <input aria-label="Quantity" required type="number" min="1" step="1" placeholder="Quantity" value={form.quantity} onChange={(e) => setForm((p) => ({ ...p, quantity: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
            <input aria-label="Unit price" required type="number" min="0" step="any" placeholder="Unit price (BDT)" value={form.unitPrice} onChange={(e) => setForm((p) => ({ ...p, unitPrice: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
          </div>
          <input aria-label="Title" required placeholder="Title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <textarea aria-label="Terms" required rows={3} placeholder="Terms" value={form.terms} onChange={(e) => setForm((p) => ({ ...p, terms: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <div className="grid gap-3 sm:grid-cols-2">
            <input aria-label="Start Date" required type="date" min={today} value={form.startDate} onChange={(e) => setForm((p) => ({ ...p, startDate: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
            <input aria-label="End Date" required type="date" min={form.startDate || today} value={form.endDate} onChange={(e) => setForm((p) => ({ ...p, endDate: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
          </div>
          <Button type="submit" variant="primary" disabled={create.isPending || !formValid}>Create Contract</Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {contracts.map((contract) => (
            <div key={contract.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{contract.title}</p>
                  <p className="text-xs text-body/60">{contract.referenceNumber} · {contract.producerName} · ৳ {contract.contractValue.toLocaleString()}</p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={statusTone[contract.status] || 'neutral'}>{contract.status}</Badge>
                  <Button variant="secondary" onClick={() => setExpandedId(expandedId === contract.id ? null : contract.id)}>
                    {expandedId === contract.id ? 'Hide' : 'Details'}
                  </Button>
                </div>
              </div>
              {expandedId === contract.id && (
                <div className="mt-4 space-y-3 border-t border-border pt-4">
                  <StatusTimeline events={contract.statusHistory} />
                  {contract.status === 'Active' && (
                    <Button variant="secondary" onClick={async () => { if (await confirmAction('Terminate this? This ends it for both sides.', { confirmLabel: 'Yes, terminate' })) terminate.mutate(contract.id); }}>Terminate</Button>
                  )}
                </div>
              )}
            </div>
          ))}
          {contracts.length === 0 && <p className="text-sm text-body/60">You haven't created any contracts yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
