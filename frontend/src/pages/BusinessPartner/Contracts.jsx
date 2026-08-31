import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState, StatusTimeline } from '../../components/ui';
import { useMyContracts, useContractMutations } from '../../hooks/useContracts';

const statusTone = { PendingApproval: 'secondary', Active: 'success', Rejected: 'neutral', Terminated: 'neutral', Expired: 'neutral' };

export default function Contracts() {
  const { data, isLoading, isError, error } = useMyContracts({ pageSize: 50 });
  const { create, terminate } = useContractMutations();
  const [expandedId, setExpandedId] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ producerId: '', title: '', terms: '', startDate: '', endDate: '' });

  const contracts = data?.items || [];

  const handleCreate = (event) => {
    event.preventDefault();
    create.mutate(
      {
        producerId: form.producerId,
        title: form.title,
        terms: form.terms,
        startDate: form.startDate,
        endDate: form.endDate,
        autoRenew: false,
        items: [],
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
          <input required placeholder="Producer ID" value={form.producerId} onChange={(e) => setForm((p) => ({ ...p, producerId: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <input required placeholder="Title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <textarea required rows={3} placeholder="Terms" value={form.terms} onChange={(e) => setForm((p) => ({ ...p, terms: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <div className="grid gap-3 sm:grid-cols-2">
            <input required type="date" value={form.startDate} onChange={(e) => setForm((p) => ({ ...p, startDate: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
            <input required type="date" value={form.endDate} onChange={(e) => setForm((p) => ({ ...p, endDate: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
          </div>
          <p className="text-xs text-body/50">Note: contract items/delivery schedule can be added after creation via a follow-up update — this form covers the core terms.</p>
          <Button type="submit" variant="primary" disabled={create.isPending}>Create Contract</Button>
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
                    <Button variant="secondary" onClick={() => terminate.mutate(contract.id)}>Terminate</Button>
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
