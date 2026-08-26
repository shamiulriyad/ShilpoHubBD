import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useReceivedQuotations, useQuotationMutations } from '../../hooks/useQuotations';

const statusTone = { Sent: 'secondary', PartiallyResponded: 'primary', Responded: 'success', Closed: 'neutral', Cancelled: 'neutral' };

export default function Quotations() {
  const { data, isLoading, isError, error } = useReceivedQuotations({ pageSize: 50 });
  const { submitResponse } = useQuotationMutations();
  const [expandedId, setExpandedId] = useState(null);
  const [form, setForm] = useState({ totalPrice: '', estimatedDeliveryDate: '', notes: '' });

  const requests = data?.items || [];

  const handleSubmit = (request) => {
    submitResponse.mutate({
      id: request.id,
      payload: {
        totalPrice: Number(form.totalPrice),
        estimatedDeliveryDate: form.estimatedDeliveryDate || undefined,
        notes: form.notes || undefined,
        items: request.items.map((item) => ({
          quotationRequestItemId: item.id,
          quotedUnitPrice: Number(form.totalPrice) / (request.items.length || 1) / (item.quantity || 1),
          quotedQuantity: item.quantity,
        })),
      },
    });
  };

  return (
    <div>
      <PageHeader title="Quotation Requests" description="RFQs from business partners looking for suppliers." />
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {requests.map((request) => (
            <div key={request.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{request.title}</p>
                  <p className="text-xs text-body/60">
                    {request.referenceNumber} · Due {new Date(request.requiredDeliveryDate).toLocaleDateString()}
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={statusTone[request.status] || 'neutral'}>{request.status}</Badge>
                  <Button variant="secondary" onClick={() => setExpandedId(expandedId === request.id ? null : request.id)}>
                    {expandedId === request.id ? 'Hide' : 'Respond'}
                  </Button>
                </div>
              </div>

              {expandedId === request.id && (
                <div className="mt-4 space-y-3 border-t border-border pt-4">
                  {request.requirements && <p className="text-sm text-body/70">{request.requirements}</p>}
                  <ul className="space-y-1 text-sm text-body/70">
                    {request.items.map((item) => (
                      <li key={item.id}>
                        {item.productName} — Qty {item.quantity}
                        {item.targetPrice ? ` (target ৳${item.targetPrice.toLocaleString()})` : ''}
                      </li>
                    ))}
                  </ul>
                  <div className="grid gap-3 sm:grid-cols-3">
                    <input
                      type="number"
                      placeholder="Total price (৳)"
                      value={form.totalPrice}
                      onChange={(event) => setForm((prev) => ({ ...prev, totalPrice: event.target.value }))}
                      className="rounded-md border border-border bg-background px-3 py-2 text-sm"
                    />
                    <input
                      type="date"
                      value={form.estimatedDeliveryDate}
                      onChange={(event) => setForm((prev) => ({ ...prev, estimatedDeliveryDate: event.target.value }))}
                      className="rounded-md border border-border bg-background px-3 py-2 text-sm"
                    />
                    <input
                      placeholder="Notes"
                      value={form.notes}
                      onChange={(event) => setForm((prev) => ({ ...prev, notes: event.target.value }))}
                      className="rounded-md border border-border bg-background px-3 py-2 text-sm"
                    />
                  </div>
                  <Button
                    variant="primary"
                    onClick={() => handleSubmit(request)}
                    disabled={!form.totalPrice || submitResponse.isPending}
                  >
                    {submitResponse.isPending ? 'Submitting…' : 'Submit Quote'}
                  </Button>
                </div>
              )}
            </div>
          ))}
          {requests.length === 0 && <p className="text-sm text-body/60">No quotation requests received yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
