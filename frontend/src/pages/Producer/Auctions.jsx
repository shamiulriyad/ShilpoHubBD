import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import TravelEmptyState from '../../components/ui/TravelEmptyState';
import MutationFeedback from '../../components/ui/MutationFeedback';
import { MyProductSelect } from '../../components/forms/EntityPickers';
import { useMyAuctions, useProducerAuctionMutations } from '../../hooks/useAuctions';

const STATUS_TONE = { Scheduled: 'secondary', Active: 'primary', Ended: 'success', Cancelled: 'neutral' };
const inputClass = 'w-full rounded-md border border-border bg-background px-3 py-2 text-sm';

// <input type="datetime-local"> gives local time with no zone; the API stores UTC.
const toUtcIso = (local) => (local ? new Date(local).toISOString() : '');
const nowLocal = () => {
  const d = new Date();
  d.setMinutes(d.getMinutes() - d.getTimezoneOffset());
  return d.toISOString().slice(0, 16);
};
const EMPTY = { productId: '', title: '', description: '', startingPrice: '', minBidIncrement: '', startAt: '', endAt: '' };

export default function ProducerAuctions() {
  const { data, isLoading, isError, error } = useMyAuctions({ pageSize: 50 });
  const { create, cancel } = useProducerAuctionMutations();
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState(EMPTY);
  const [cancelId, setCancelId] = useState(null);
  const auctions = data?.items || [];
  const set = (key) => (e) => setForm((f) => ({ ...f, [key]: e.target.value }));

  const startMs = form.startAt ? new Date(form.startAt).getTime() : NaN;
  const endMs = form.endAt ? new Date(form.endAt).getTime() : NaN;
  const timesOk = Number.isFinite(startMs) && Number.isFinite(endMs) && endMs > startMs && endMs > Date.now();
  const valid = Boolean(
    form.productId && form.title.trim() && form.description.trim()
    && Number(form.startingPrice) > 0 && Number(form.minBidIncrement) > 0 && timesOk,
  );

  const missing = [
    !form.productId && 'a product',
    !form.title.trim() && 'a title',
    !form.description.trim() && 'a description',
    !(Number(form.startingPrice) > 0) && 'a starting price',
    !(Number(form.minBidIncrement) > 0) && 'a minimum bid increase',
    !form.startAt && 'a start date and time (pick both the date and the time)',
    !form.endAt && 'an end date and time (pick both the date and the time)',
  ].filter(Boolean);

  const submit = (event) => {
    event.preventDefault();
    if (!valid) return;
    create.mutate(
      {
        productId: form.productId,
        title: form.title.trim(),
        description: form.description.trim(),
        startingPrice: Number(form.startingPrice),
        minBidIncrement: Number(form.minBidIncrement),
        startAt: toUtcIso(form.startAt),
        endAt: toUtcIso(form.endAt),
      },
      { onSuccess: () => { setForm(EMPTY); setShowForm(false); } },
    );
  };

  return (
    <div>
      <PageHeader
        title="Auctions"
        description="Put a rare or limited piece up for bids. Customers see it under Auctions once it starts."
        action={<Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Close' : 'New auction'}</Button>}
      />

      {showForm && (
        <form onSubmit={submit} className="mb-6 grid gap-4 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <div className="sm:col-span-2">
            <MyProductSelect id="auction-product" label="Product to auction" required value={form.productId} onChange={(v) => setForm((f) => ({ ...f, productId: v }))} />
          </div>
          <label className="block text-sm sm:col-span-2">
            <span className="mb-1 block font-medium text-heading">Title</span>
            <input required maxLength={200} value={form.title} onChange={set('title')} placeholder="e.g. Hand-woven Jamdani, limited edition" className={inputClass} />
          </label>
          <label className="block text-sm sm:col-span-2">
            <span className="mb-1 block font-medium text-heading">Description</span>
            <textarea required rows={3} maxLength={2000} value={form.description} onChange={set('description')} placeholder="What makes this piece special?" className={inputClass} />
          </label>
          <label className="block text-sm">
            <span className="mb-1 block font-medium text-heading">Starting price (৳)</span>
            <input required type="number" min="1" step="any" value={form.startingPrice} onChange={set('startingPrice')} className={inputClass} />
          </label>
          <label className="block text-sm">
            <span className="mb-1 block font-medium text-heading">Minimum bid increase (৳)</span>
            <input required type="number" min="1" step="any" value={form.minBidIncrement} onChange={set('minBidIncrement')} className={inputClass} />
          </label>
          <label className="block text-sm">
            <span className="mb-1 block font-medium text-heading">Starts</span>
            <input required type="datetime-local" min={nowLocal()} value={form.startAt} onChange={set('startAt')} className={inputClass} />
          </label>
          <label className="block text-sm">
            <span className="mb-1 block font-medium text-heading">Ends</span>
            <input required type="datetime-local" min={form.startAt || nowLocal()} value={form.endAt} onChange={set('endAt')} className={inputClass} />
          </label>
          {form.startAt && form.endAt && !timesOk && (
            <p role="alert" className="text-sm text-error sm:col-span-2">The end time must be after the start time and in the future.</p>
          )}
          {!valid && missing.length > 0 && (
            <p className="text-sm text-body/70 sm:col-span-2">Still needed: {missing.join(', ')}.</p>
          )}
          <div className="sm:col-span-2">
            <Button type="submit" variant="primary" disabled={!valid || create.isPending}>{create.isPending ? 'Creating…' : 'Create auction'}</Button>
          </div>
        </form>
      )}
      <MutationFeedback mutation={create} successMessage="Auction created." />

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {auctions.map((a) => (
            <article key={a.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{a.title}</p>
                  <p className="text-xs text-body/60">
                    {a.productName} · {new Date(a.startAt).toLocaleString()} → {new Date(a.endAt).toLocaleString()}
                  </p>
                </div>
                <Badge tone={STATUS_TONE[a.status] || 'neutral'}>{a.status}</Badge>
              </div>
              <p className="mt-2 text-sm text-body/80">
                Current bid <span className="font-semibold text-heading">৳ {Number(a.currentPrice).toLocaleString('en-BD')}</span> · {a.bidCount} bid{a.bidCount === 1 ? '' : 's'}
                {a.status === 'Ended' && (a.winnerName ? ` · Won by ${a.winnerName}` : ' · No bids')}
              </p>
              {['Scheduled', 'Active'].includes(a.status) && (
                <div className="mt-3">
                  {cancelId === a.id ? (
                    <div className="flex flex-wrap items-center gap-2 text-sm">
                      <span>Cancel this auction?</span>
                      <Button variant="secondary" onClick={() => setCancelId(null)}>Keep</Button>
                      <Button onClick={() => cancel.mutate(a.id, { onSuccess: () => setCancelId(null) })} disabled={cancel.isPending}>Yes, cancel</Button>
                    </div>
                  ) : (
                    <Button variant="secondary" onClick={() => setCancelId(a.id)}>Cancel auction</Button>
                  )}
                </div>
              )}
            </article>
          ))}
          {auctions.length === 0 && <TravelEmptyState title="No auctions yet" description="Create your first auction to let customers bid on one of your pieces." />}
        </div>
      </AsyncState>
      <div className="mt-4"><MutationFeedback mutation={cancel} successMessage="Auction cancelled." /></div>
    </div>
  );
}
