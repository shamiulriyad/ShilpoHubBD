import { useState } from 'react';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button, AsyncState } from '../../components/ui';
import { useProducts, useProduct } from '../../hooks/useProducts';
import { useMyCustomOrders, useCustomOrderMutations } from '../../hooks/useCustomOrders';

const statusLabel = {
  0: 'Pending',
  1: 'Quoted',
  2: 'Accepted',
  3: 'Declined',
  4: 'Cancelled',
  Pending: 'Pending',
  Quoted: 'Quoted',
  Accepted: 'Accepted',
  Declined: 'Declined',
  Cancelled: 'Cancelled',
};

export default function CustomOrder() {
  const { data: productPage } = useProducts({ pageSize: 50 });
  const products = productPage?.items || [];

  const [form, setForm] = useState({
    productId: '',
    title: '',
    specifications: '',
    budget: '',
    deadline: '',
  });

  // The selected product carries the producerId the API requires.
  const selectedProduct = useProduct(form.productId);
  const myRequests = useMyCustomOrders();
  const { create, cancel } = useCustomOrderMutations();

  const setField = (key) => (e) => setForm((f) => ({ ...f, [key]: e.target.value }));

  const handleSubmit = (event) => {
    event.preventDefault();
    const producerId = selectedProduct.data?.producerId;
    if (!producerId || !form.title.trim() || !form.specifications.trim()) return;

    create.mutate(
      {
        producerId,
        productId: form.productId,
        title: form.title.trim(),
        specifications: form.specifications.trim(),
        budget: form.budget ? Number(form.budget) : null,
        deadline: form.deadline ? new Date(form.deadline).toISOString() : null,
      },
      {
        onSuccess: () => setForm({ productId: '', title: '', specifications: '', budget: '', deadline: '' }),
      },
    );
  };

  return (
    <div>
      <PageHeader
        breadcrumbs={[
          { label: 'Dashboard', path: routePaths.customer },
          { label: 'Marketplace', path: routePaths.customerMarketplace },
          { label: 'Custom Order' },
        ]}
        title="Request a Custom Order"
        description="Commission a bespoke piece directly from a heritage producer, made to your specifications."
      />

      <form onSubmit={handleSubmit} className="grid gap-8 lg:grid-cols-[2fr_1fr]">
        <div className="space-y-6 rounded-xl border border-border bg-surface p-6">
          <div>
            <p className="mb-3 text-sm font-semibold text-heading">Reference Product & Producer</p>
            <select
              required
              value={form.productId}
              onChange={setField('productId')}
              className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
            >
              <option value="">Select a product to base your request on…</option>
              {products.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.name} — {p.producerName}
                </option>
              ))}
            </select>
            {form.productId && selectedProduct.data && (
              <p className="mt-2 text-xs text-body/60">
                Request will go to <span className="font-medium">{selectedProduct.data.producerName}</span>.
              </p>
            )}
          </div>

          <div>
            <p className="mb-3 text-sm font-semibold text-heading">Order Details</p>
            <div className="grid gap-4">
              <input
                required
                value={form.title}
                onChange={setField('title')}
                placeholder="Item title (e.g. Custom Jamdani Saree)"
                className="rounded-md border border-border bg-background px-3 py-2 text-sm"
              />
              <textarea
                required
                value={form.specifications}
                onChange={setField('specifications')}
                placeholder="Describe colors, size, materials and any inspiration references…"
                rows={5}
                className="rounded-md border border-border bg-background px-3 py-2 text-sm"
              />
              <div className="grid gap-4 sm:grid-cols-2">
                <input
                  type="number"
                  min="0"
                  value={form.budget}
                  onChange={setField('budget')}
                  placeholder="Budget (৳)"
                  className="rounded-md border border-border bg-background px-3 py-2 text-sm"
                />
                <input
                  type="date"
                  value={form.deadline}
                  onChange={setField('deadline')}
                  className="rounded-md border border-border bg-background px-3 py-2 text-sm"
                />
              </div>
            </div>
          </div>

          {create.isError && (
            <p className="text-sm text-red-600">
              {create.error?.response?.data?.title || 'Could not submit your request. Please try again.'}
            </p>
          )}
          {create.isSuccess && <p className="text-sm text-success">Request submitted — the producer will respond with a quote.</p>}
        </div>

        <div className="h-fit space-y-3 rounded-xl border border-border bg-surface p-5">
          <p className="text-sm font-semibold text-heading">How Custom Orders Work</p>
          <ol className="space-y-2 text-sm text-body/70">
            <li>1. Submit your request with details and budget.</li>
            <li>2. A matching producer reviews and sends a quote.</li>
            <li>3. Approve the quote to begin production.</li>
            <li>4. Track progress until delivery.</li>
          </ol>
          <Button type="submit" variant="primary" className="w-full" disabled={create.isPending}>
            {create.isPending ? 'Submitting…' : 'Submit Request'}
          </Button>
        </div>
      </form>

      <div className="mt-10">
        <p className="mb-3 text-sm font-semibold text-heading">My Custom Order Requests</p>
        <AsyncState isLoading={myRequests.isLoading} isError={myRequests.isError} error={myRequests.error}>
          <div className="divide-y divide-border rounded-xl border border-border bg-surface">
            {(myRequests.data || []).map((req) => (
              <div key={req.id} className="flex flex-wrap items-center justify-between gap-2 p-4">
                <div>
                  <p className="text-sm font-medium text-heading">{req.title}</p>
                  <p className="text-xs text-body/60">
                    {req.producerName} · {new Date(req.createdAt).toLocaleDateString()}
                    {req.quotedPrice ? ` · Quoted ৳ ${req.quotedPrice.toLocaleString()}` : ''}
                  </p>
                </div>
                <div className="flex items-center gap-3">
                  <span className="text-xs font-medium text-body/70">
                    {statusLabel[req.status] ?? req.status}
                  </span>
                  {(statusLabel[req.status] === 'Pending' || statusLabel[req.status] === 'Quoted') && (
                    <button
                      type="button"
                      onClick={() => cancel.mutate(req.id)}
                      className="text-xs font-medium text-red-600 hover:underline"
                    >
                      Cancel
                    </button>
                  )}
                </div>
              </div>
            ))}
            {(myRequests.data || []).length === 0 && (
              <p className="p-4 text-sm text-body/60">You have no custom order requests yet.</p>
            )}
          </div>
        </AsyncState>
      </div>
    </div>
  );
}
