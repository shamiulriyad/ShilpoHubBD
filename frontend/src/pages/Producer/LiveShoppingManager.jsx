import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useLiveEvents, useMyLiveEventMutations } from '../../hooks/useLiveEvents';
import { useMyProducts } from '../../hooks/useProducts';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const statusTone = { Scheduled: 'neutral', Live: 'primary', Ended: 'success', Cancelled: 'neutral' };

export default function LiveShoppingManager() {
  const { data, isLoading, isError, error } = useLiveEvents({ pageSize: 50 });
  const productsQuery = useMyProducts();
  const { create, start, end } = useMyLiveEventMutations();
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ productId: '', title: '', description: '', scheduledStartAt: '' });

  const events = data?.items || [];

  const handleCreate = (event) => {
    event.preventDefault();
    create.mutate(
      { ...form, scheduledStartAt: new Date(form.scheduledStartAt).toISOString() },
      { onSuccess: () => { setShowForm(false); setForm({ productId: '', title: '', description: '', scheduledStartAt: '' }); } },
    );
  };

  return (
    <div>
      <PageHeader
        title="Live Shopping"
        description="Schedule and run live shopping events for your products."
        action={<Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'Schedule Event'}</Button>}
      />

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <select required value={form.productId} onChange={(e) => setForm((p) => ({ ...p, productId: e.target.value }))} className={inputClass}>
            <option value="">Select product</option>
            {(productsQuery.data || []).map((p) => <option key={p.id} value={p.id}>{p.name}</option>)}
          </select>
          <input required type="datetime-local" value={form.scheduledStartAt} onChange={(e) => setForm((p) => ({ ...p, scheduledStartAt: e.target.value }))} className={inputClass} />
          <input required placeholder="Title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <textarea required rows={2} placeholder="Description" value={form.description} onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={create.isPending}>
            {create.isPending ? 'Scheduling…' : 'Schedule Event'}
          </Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {events.map((e) => (
            <div key={e.id} className="flex flex-wrap items-center justify-between gap-2 rounded-xl border border-border bg-surface p-4">
              <div>
                <p className="text-sm font-semibold text-heading">{e.title}</p>
                <p className="text-xs text-body/60">{new Date(e.scheduledStartAt).toLocaleString()} · {e.commentCount} comments · {e.purchaseCount} purchases</p>
              </div>
              <div className="flex items-center gap-2">
                <Badge tone={statusTone[e.status] || 'neutral'}>{e.status}</Badge>
                {e.status === 'Scheduled' && (
                  <Button size="sm" variant="primary" disabled={start.isPending} onClick={() => start.mutate(e.id)}>Go Live</Button>
                )}
                {e.status === 'Live' && (
                  <Button size="sm" variant="secondary" disabled={end.isPending} onClick={() => end.mutate(e.id)}>End</Button>
                )}
              </div>
            </div>
          ))}
          {events.length === 0 && <p className="text-sm text-body/60">No live shopping events scheduled yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
