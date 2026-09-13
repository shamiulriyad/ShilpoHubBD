import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState, Pagination } from '../../components/ui';
import { useMyLiveEvents, useMyLiveEventMutations } from '../../hooks/useLiveEvents';
import { useMyProducts } from '../../hooks/useProducts';
import { getApiErrorMessage } from '../../utils/apiError';

const inputClass = 'w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-body focus:border-primary focus:outline-none focus:ring-4 focus:ring-primary/10';
const statusTone = { Scheduled: 'secondary', Live: 'success', Ended: 'neutral', Cancelled: 'neutral' };

export default function LiveShoppingManager() {
  const [page, setPage] = useState(1);
  const eventsQuery = useMyLiveEvents({ page, pageSize: 20 });
  const productsQuery = useMyProducts();
  const { create, start, end } = useMyLiveEventMutations();
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ productId: '', title: '', description: '', scheduledStartAt: '' });
  const [actionError, setActionError] = useState('');

  const events = eventsQuery.data?.items || [];
  const products = productsQuery.data || [];

  const handleCreate = (event) => {
    event.preventDefault();
    if (create.isPending) return;
    setActionError('');
    create.mutate(
      { ...form, scheduledStartAt: new Date(form.scheduledStartAt).toISOString() },
      {
        onSuccess: () => {
          setShowForm(false);
          setForm({ productId: '', title: '', description: '', scheduledStartAt: '' });
        },
        onError: (error) => setActionError(getApiErrorMessage(error, 'Unable to schedule the live event.')),
      },
    );
  };

  const runAction = (mutation, id, fallback) => {
    if (mutation.isPending) return;
    setActionError('');
    mutation.mutate(id, { onError: (error) => setActionError(getApiErrorMessage(error, fallback)) });
  };

  return (
    <div>
      <PageHeader
        title="Live Shopping"
        description="Schedule and manage live-commerce events for products you own."
        action={
          <Button type="button" variant="primary" onClick={() => { setShowForm((visible) => !visible); setActionError(''); }}>
            {showForm ? 'Cancel' : 'Schedule Event'}
          </Button>
        }
      />

      {actionError && <p role="alert" className="mb-4 rounded-lg border border-error/30 bg-error/10 px-3 py-2 text-sm text-error">{actionError}</p>}

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 grid gap-4 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <div>
            <label htmlFor="live-product" className="mb-1.5 block text-sm font-medium text-body/70">Product</label>
            <select aria-label="Product Id"
              id="live-product"
              required
              value={form.productId}
              onChange={(event) => setForm((previous) => ({ ...previous, productId: event.target.value }))}
              className={inputClass}
              disabled={productsQuery.isLoading}
            >
              <option value="">{productsQuery.isLoading ? 'Loading products…' : 'Select product'}</option>
              {products.map((product) => <option key={product.id} value={product.id}>{product.name}</option>)}
            </select>
            {productsQuery.isError && <p className="mt-1 text-xs text-error">{getApiErrorMessage(productsQuery.error, 'Unable to load your products.')}</p>}
          </div>
          <div>
            <label htmlFor="live-scheduled" className="mb-1.5 block text-sm font-medium text-body/70">Scheduled start</label>
            <input aria-label="Scheduled Start At"
              id="live-scheduled"
              required
              type="datetime-local"
              value={form.scheduledStartAt}
              onChange={(event) => setForm((previous) => ({ ...previous, scheduledStartAt: event.target.value }))}
              className={inputClass}
            />
          </div>
          <div className="sm:col-span-2">
            <label htmlFor="live-title" className="mb-1.5 block text-sm font-medium text-body/70">Title</label>
            <input aria-label="Title"
              id="live-title"
              required
              maxLength={200}
              value={form.title}
              onChange={(event) => setForm((previous) => ({ ...previous, title: event.target.value }))}
              className={inputClass}
            />
          </div>
          <div className="sm:col-span-2">
            <label htmlFor="live-description" className="mb-1.5 block text-sm font-medium text-body/70">Description</label>
            <textarea aria-label="Description"
              id="live-description"
              required
              maxLength={2000}
              rows={3}
              value={form.description}
              onChange={(event) => setForm((previous) => ({ ...previous, description: event.target.value }))}
              className={inputClass}
            />
          </div>
          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={create.isPending || products.length === 0}>
            {create.isPending ? 'Scheduling…' : products.length === 0 ? 'Add a product before scheduling' : 'Schedule Event'}
          </Button>
        </form>
      )}

      <AsyncState isLoading={eventsQuery.isLoading} isError={eventsQuery.isError} error={eventsQuery.error} loadingText="Loading your live events…">
        <div className="space-y-3">
          {events.map((event) => (
            <div key={event.id} className="flex flex-wrap items-center justify-between gap-3 rounded-xl border border-border bg-surface p-4">
              <div className="min-w-0">
                <p className="break-words text-sm font-semibold text-heading">{event.title}</p>
                <p className="mt-1 text-xs text-body/60">{event.productName} · {new Date(event.scheduledStartAt).toLocaleString()}</p>
                <p className="mt-1 text-xs text-body/50">{event.reactionCount ?? 0} reactions</p>
              </div>
              <div className="flex flex-wrap items-center gap-2">
                <Badge tone={statusTone[event.status] || 'neutral'}>{event.status}</Badge>
                {event.status === 'Scheduled' && (
                  <Button size="sm" variant="primary" disabled={start.isPending} onClick={() => runAction(start, event.id, 'Unable to start this event.')}>
                    {start.isPending ? 'Starting…' : 'Go Live'}
                  </Button>
                )}
                {event.status === 'Live' && (
                  <Button size="sm" variant="secondary" disabled={end.isPending} onClick={() => runAction(end, event.id, 'Unable to end this event.')}>
                    {end.isPending ? 'Ending…' : 'End Event'}
                  </Button>
                )}
              </div>
            </div>
          ))}
          {events.length === 0 && <p className="rounded-xl border border-dashed border-border p-6 text-sm text-body/60">You have not scheduled any live shopping events yet.</p>}
        </div>
      </AsyncState>

      {(eventsQuery.data?.totalPages || 0) > 1 && (
        <div className="mt-8"><Pagination currentPage={page} totalPages={eventsQuery.data.totalPages} onPageChange={setPage} /></div>
      )}
    </div>
  );
}
