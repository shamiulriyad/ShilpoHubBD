import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import TravelEmptyState from '../../components/ui/TravelEmptyState';
import MutationFeedback from '../../components/ui/MutationFeedback';
import { useProducerCustomOrders, useCustomOrderMutations } from '../../hooks/useCustomOrders';

const STATUS_LABEL = {
  Pending: 'Pending',
  Accepted: 'Accepted',
  Rejected: 'Rejected',
  InProgress: 'In progress',
  Completed: 'Completed',
  Cancelled: 'Cancelled',
};
const STATUS_TONE = { Pending: 'secondary', Accepted: 'primary', InProgress: 'primary', Completed: 'success', Rejected: 'neutral', Cancelled: 'neutral' };

const inputClass = 'w-full rounded-md border border-border bg-background px-3 py-2 text-sm';

function ResponseForm({ order, respond }) {
  const [price, setPrice] = useState(order.quotedPrice ?? '');
  const [message, setMessage] = useState('');
  const priceOk = price !== '' && Number(price) > 0;

  const send = (status) =>
    respond.mutate({
      id: order.id,
      payload: {
        status,
        quotedPrice: status === 'Accepted' && priceOk ? Number(price) : undefined,
        responseMessage: message.trim() || undefined,
      },
    });

  return (
    <div className="mt-4 grid gap-3 border-t border-border pt-4 sm:grid-cols-[1fr_2fr]">
      <label className="block text-sm">
        <span className="mb-1 block font-medium text-heading">Your price (৳)</span>
        <input type="number" min="1" step="any" placeholder="Quote for this piece" value={price} onChange={(e) => setPrice(e.target.value)} className={inputClass} />
      </label>
      <label className="block text-sm">
        <span className="mb-1 block font-medium text-heading">Message to the customer</span>
        <input placeholder="Timeline, materials, questions…" value={message} onChange={(e) => setMessage(e.target.value)} className={inputClass} />
      </label>
      <div className="flex flex-wrap gap-2 sm:col-span-2">
        <Button variant="primary" onClick={() => send('Accepted')} disabled={!priceOk || respond.isPending}>
          Accept &amp; send quote
        </Button>
        <Button variant="secondary" onClick={() => send('Rejected')} disabled={respond.isPending}>
          Decline
        </Button>
        {!priceOk && <span className="self-center text-xs text-body/60">Enter a price to accept.</span>}
      </div>
    </div>
  );
}

export default function ProducerCustomOrders() {
  const { data, isLoading, isError, error } = useProducerCustomOrders();
  const { respond } = useCustomOrderMutations();
  const orders = data || [];

  return (
    <div>
      <PageHeader
        title="Custom Orders"
        description="Bespoke pieces customers have asked you to make. Send a quote, then follow the work through to completion."
      />

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-4">
          {orders.map((order) => (
            <article key={order.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-start justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{order.title}</p>
                  <p className="text-xs text-body/60">
                    From {order.customerName} · {new Date(order.createdAt).toLocaleDateString()}
                    {order.productName ? ` · Based on ${order.productName}` : ''}
                  </p>
                </div>
                <Badge tone={STATUS_TONE[order.status] || 'neutral'}>{STATUS_LABEL[order.status] || order.status}</Badge>
              </div>

              <p className="mt-3 whitespace-pre-line text-sm text-body/80">{order.specifications}</p>
              <p className="mt-2 text-xs text-body/60">
                {order.budget != null ? `Customer budget: ৳ ${Number(order.budget).toLocaleString('en-BD')}` : 'No budget given'}
                {order.deadline ? ` · Needed by ${new Date(order.deadline).toLocaleDateString()}` : ''}
                {order.quotedPrice != null ? ` · Your quote: ৳ ${Number(order.quotedPrice).toLocaleString('en-BD')}` : ''}
              </p>
              {order.producerResponse && <p className="mt-1 text-xs text-body/70">Your message: “{order.producerResponse}”</p>}

              {order.status === 'Pending' && <ResponseForm order={order} respond={respond} />}
              {order.status === 'Accepted' && (
                <div className="mt-4 border-t border-border pt-4">
                  <Button variant="primary" onClick={() => respond.mutate({ id: order.id, payload: { status: 'InProgress', quotedPrice: order.quotedPrice } })} disabled={respond.isPending}>
                    Start work
                  </Button>
                </div>
              )}
              {order.status === 'InProgress' && (
                <div className="mt-4 border-t border-border pt-4">
                  <Button variant="primary" onClick={() => respond.mutate({ id: order.id, payload: { status: 'Completed', quotedPrice: order.quotedPrice } })} disabled={respond.isPending}>
                    Mark completed
                  </Button>
                </div>
              )}
            </article>
          ))}
          {orders.length === 0 && (
            <TravelEmptyState title="No custom orders yet" description="When a customer requests a bespoke piece from you, it will appear here." />
          )}
        </div>
      </AsyncState>
      <div className="mt-4">
        <MutationFeedback mutation={respond} successMessage="Custom order updated." />
      </div>
    </div>
  );
}
