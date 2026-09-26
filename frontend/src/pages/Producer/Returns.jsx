import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState, ConfirmDialog } from '../../components/ui';
import MutationFeedback from '../../components/ui/MutationFeedback';
import TravelEmptyState from '../../components/ui/TravelEmptyState';
import { useProducerReturns, useProducerReturnMutations } from '../../hooks/useProducerReturns';

const money = (v) => `৳ ${Number(v ?? 0).toLocaleString('en-BD')}`;

const STATUS_LABEL = {
  ReturnRequested: 'Waiting for your decision',
  Returned: 'Returned to you',
  Refunded: 'Returned and customer refunded',
};
const STATUS_TONE = { ReturnRequested: 'secondary', Returned: 'success', Refunded: 'success' };

function ReturnCard({ item, onAccept, onReject, busy }) {
  const [note, setNote] = useState('');
  const [confirmAccept, setConfirmAccept] = useState(false);

  return (
    <article className="rounded-xl border border-border bg-surface p-4">
      <div className="flex flex-wrap items-start justify-between gap-2">
        <div>
          <p className="text-sm font-semibold text-heading">{item.orderNumber} · {item.customerName}</p>
          <p className="text-xs text-body/60">Requested {new Date(item.requestedAt).toLocaleDateString()} · {item.customerPhone}</p>
        </div>
        <Badge tone={STATUS_TONE[item.orderStatus] || 'neutral'}>{STATUS_LABEL[item.orderStatus] || item.orderStatus}</Badge>
      </div>

      {item.reason && <p className="mt-3 text-sm text-body/80">Reason: “{item.reason}”</p>}

      <ul className="mt-3 space-y-1 text-sm text-body/80">
        {item.items.map((line, i) => (
          <li key={i} className="flex justify-between"><span>{line.productName} × {line.quantity}</span><span>{money(line.lineTotal)}</span></li>
        ))}
      </ul>

      <dl className="mt-3 grid grid-cols-2 gap-2 border-t border-border pt-3 text-xs sm:grid-cols-4">
        <div><dt className="text-body/60">Return amount (your items)</dt><dd className="font-semibold text-heading">{money(item.itemsAmount)}</dd></div>
        <div><dt className="text-body/60">Order total</dt><dd className="font-semibold text-heading">{money(item.orderTotal)}</dd></div>
        <div><dt className="text-body/60">Paid by customer</dt><dd className="font-semibold text-heading">{money(item.amountPaid)}</dd></div>
        <div><dt className="text-body/60">Refunded</dt><dd className="font-semibold text-heading">{item.refundedAmount != null ? money(item.refundedAmount) : '—'}</dd></div>
      </dl>

      {item.returnReference && (
        <p className="mt-3 text-sm text-body/70">
          Return pickup {item.returnReference}: <span className="font-medium text-heading">{item.returnStatus}</span>.
          {item.orderStatus === 'ReturnRequested' && ' The customer is refunded as soon as the goods reach you.'}
        </p>
      )}

      {item.canRespond && (
        <div className="mt-4 flex flex-wrap items-center gap-2 border-t border-border pt-4">
          <input aria-label="Reason if you reject" placeholder="Reason if you reject (optional)" value={note} onChange={(e) => setNote(e.target.value)} className="min-w-[14rem] flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <Button variant="primary" disabled={busy} onClick={() => setConfirmAccept(true)}>Accept return</Button>
          <Button variant="secondary" disabled={busy} onClick={() => onReject(item.orderId, note.trim() || undefined)}>Reject</Button>
        </div>
      )}

      <ConfirmDialog
        open={confirmAccept}
        title="Accept this return?"
        message={`A logistics partner will collect the goods from ${item.customerName} and bring them back to you. When they arrive, the customer is refunded ${item.amountPaid > 0 ? money(item.amountPaid) : 'anything they paid (nothing paid in advance)'}.`}
        confirmLabel="Accept return"
        cancelLabel="Not yet"
        busy={busy}
        onConfirm={() => { setConfirmAccept(false); onAccept(item.orderId); }}
        onCancel={() => setConfirmAccept(false)}
      />
    </article>
  );
}

export default function ProducerReturns() {
  const { data, isLoading, isError, error } = useProducerReturns();
  const { accept, reject } = useProducerReturnMutations();
  const returns = data || [];
  const busy = accept.isPending || reject.isPending;

  return (
    <div>
      <PageHeader
        title="Returns"
        description="Customers who asked to return an order. Accept to have the logistics partner bring the goods back; the customer is refunded when they arrive."
      />
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-4">
          {returns.map((r) => (
            <ReturnCard
              key={r.orderId}
              item={r}
              busy={busy}
              onAccept={(orderId) => accept.mutate(orderId)}
              onReject={(orderId, note) => reject.mutate({ orderId, note })}
            />
          ))}
          {returns.length === 0 && <TravelEmptyState title="No returns" description="When a customer asks to return one of your orders, it will appear here." />}
        </div>
      </AsyncState>
      <div className="mt-4">
        <MutationFeedback mutation={accept} successMessage="Return accepted. The logistics partner has been asked to collect it." />
        <MutationFeedback mutation={reject} successMessage="Return rejected. The customer has been told." />
      </div>
    </div>
  );
}
