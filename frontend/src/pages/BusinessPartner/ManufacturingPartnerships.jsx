import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState, MilestoneList } from '../../components/ui';
import { useMyPartnerships, usePartnershipMutations } from '../../hooks/useManufacturingPartnerships';

const statusTone = { Requested: 'secondary', Accepted: 'primary', Rejected: 'neutral', InProgress: 'primary', Completed: 'success', Cancelled: 'neutral' };

export default function ManufacturingPartnerships() {
  const { data, isLoading, isError, error } = useMyPartnerships({ pageSize: 50 });
  const { create, updateMilestoneStatus, complete, cancel } = usePartnershipMutations();
  const [expandedId, setExpandedId] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ producerId: '', title: '', productRequirements: '', manufacturingSpecifications: '', quantity: '', timelineStartDate: '', timelineEndDate: '' });

  const partnerships = data?.items || [];

  const handleCreate = (event) => {
    event.preventDefault();
    create.mutate(
      { ...form, quantity: Number(form.quantity), milestones: [] },
      { onSuccess: () => setShowForm(false) },
    );
  };

  return (
    <div>
      <PageHeader
        title="Manufacturing Partnerships"
        description="Contract manufacturing requests you've sent to producers."
        action={<Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'New Partnership'}</Button>}
      />

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 space-y-3 rounded-xl border border-border bg-surface p-4">
          <input required placeholder="Producer ID" value={form.producerId} onChange={(e) => setForm((p) => ({ ...p, producerId: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <input required placeholder="Title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <textarea required rows={2} placeholder="Product requirements" value={form.productRequirements} onChange={(e) => setForm((p) => ({ ...p, productRequirements: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <textarea required rows={2} placeholder="Manufacturing specifications" value={form.manufacturingSpecifications} onChange={(e) => setForm((p) => ({ ...p, manufacturingSpecifications: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <div className="grid gap-3 sm:grid-cols-3">
            <input required type="number" placeholder="Quantity" value={form.quantity} onChange={(e) => setForm((p) => ({ ...p, quantity: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
            <input required type="date" value={form.timelineStartDate} onChange={(e) => setForm((p) => ({ ...p, timelineStartDate: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
            <input required type="date" value={form.timelineEndDate} onChange={(e) => setForm((p) => ({ ...p, timelineEndDate: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
          </div>
          <Button type="submit" variant="primary" disabled={create.isPending}>Send Request</Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {partnerships.map((p) => (
            <div key={p.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{p.title}</p>
                  <p className="text-xs text-body/60">{p.producerName} · Qty {p.quantity} · {p.progressPercentage}% complete</p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={statusTone[p.status] || 'neutral'}>{p.status}</Badge>
                  <Button variant="secondary" onClick={() => setExpandedId(expandedId === p.id ? null : p.id)}>
                    {expandedId === p.id ? 'Hide' : 'Details'}
                  </Button>
                </div>
              </div>
              {expandedId === p.id && (
                <div className="mt-4 space-y-3 border-t border-border pt-4">
                  <MilestoneList milestones={p.milestones} onAdvance={(m) => updateMilestoneStatus.mutate({ id: p.id, milestoneId: m.id, status: 'Completed' })} />
                  <div className="flex gap-2">
                    {['Accepted', 'InProgress'].includes(p.status) && (
                      <Button variant="primary" onClick={() => complete.mutate(p.id)}>Mark Complete</Button>
                    )}
                    {!['Completed', 'Cancelled'].includes(p.status) && (
                      <Button variant="secondary" onClick={() => cancel.mutate(p.id)}>Cancel</Button>
                    )}
                  </div>
                </div>
              )}
            </div>
          ))}
          {partnerships.length === 0 && <p className="text-sm text-body/60">You haven't requested any manufacturing partnerships yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
