import { useState } from 'react';
import { Pagination, PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useQuotation, useReceivedQuotations, useQuotationMutations } from '../../hooks/useQuotations';

import MutationFeedback from '../../components/ui/MutationFeedback';

const statusTone = { Sent: 'secondary', PartiallyResponded: 'primary', Responded: 'success', Closed: 'neutral', Cancelled: 'neutral' };

export default function Quotations() {
  const [page, setPage] = useState(1);
  const { data, isLoading, isError, error } = useReceivedQuotations({ page, pageSize: 10 });
  const { submitResponse } = useQuotationMutations();
  const [expandedId, setExpandedId] = useState(null);
  const detailQuery = useQuotation(expandedId);
  const [form, setForm] = useState({ unitPrices: {}, estimatedDeliveryDate: '', notes: '' });

  const requests = data?.items || [];

  const handleSubmit = (request) => {
    submitResponse.mutate({
      id: request.id,
      payload: {
        totalPrice: request.items.reduce((sum,item) => sum + Number(form.unitPrices[item.id] || 0) * item.quantity, 0),
        estimatedDeliveryDate: form.estimatedDeliveryDate ? `${form.estimatedDeliveryDate}T00:00:00.000Z` : undefined,
        notes: form.notes || undefined,
        items: (request.items || []).map((item) => ({
          quotationRequestItemId: item.id,
          quotedUnitPrice: Number(form.unitPrices[item.id]),
          quotedQuantity: item.quantity,
        })),
      },
    });
  };

  return (
    <div>
      <PageHeader title="Quotation Requests" description="RFQs from business partners looking for suppliers." />
<div className="mb-4 space-y-2"><MutationFeedback mutation={submitResponse} successMessage="Changes saved." /></div>
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {requests.map((summary) => {
            const request = expandedId === summary.id && detailQuery.data ? detailQuery.data : summary;
            return (
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
                  <Button variant="secondary" onClick={() => { setExpandedId(expandedId === request.id ? null : request.id); setForm({unitPrices:{},estimatedDeliveryDate:'',notes:''}); submitResponse.reset(); }}>
                    {expandedId === request.id ? 'Hide' : 'Respond'}
                  </Button>
                </div>
              </div>

              {expandedId === request.id && (
                <AsyncState isLoading={detailQuery.isLoading} isError={detailQuery.isError} error={detailQuery.error}>
                <div className="mt-4 space-y-3 border-t border-border pt-4">
                  {request.requirements && <p className="text-sm text-body/70">{request.requirements}</p>}
                  <ul className="space-y-1 text-sm text-body/70">
                    {(request.items || []).map((item) => (
                      <li key={item.id}>
                        {item.productName} — Qty {item.quantity}
                        {item.targetPrice ? ` (target ৳${item.targetPrice.toLocaleString()})` : ''}
                      </li>
                    ))}
                  </ul>
                  <div className="grid gap-3 sm:grid-cols-3">
                    {(request.items || []).map(item => <label key={item.id} className="text-sm">{item.productName} · unit price (৳)<input aria-label={`Unit price for ${item.productName}`} type="number" min="0" step="0.01" required value={form.unitPrices[item.id] ?? ''} onChange={event=>setForm(previous=>({...previous,unitPrices:{...previous.unitPrices,[item.id]:event.target.value}}))} className="mt-1 w-full rounded-md border border-border bg-background px-3 py-2 text-sm" /></label>)}
                    <p className="text-sm font-semibold sm:col-span-3">Quote total: ৳ {(request.items || []).reduce((sum,item)=>sum+Number(form.unitPrices[item.id] || 0)*item.quantity,0).toLocaleString()}</p>
                    <input aria-label="Estimated Delivery Date"
                      type="date"
                      value={form.estimatedDeliveryDate}
                      onChange={(event) => setForm((prev) => ({ ...prev, estimatedDeliveryDate: event.target.value }))}
                      className="rounded-md border border-border bg-background px-3 py-2 text-sm"
                    />
                    <input aria-label="Notes"
                      placeholder="Notes"
                      value={form.notes}
                      onChange={(event) => setForm((prev) => ({ ...prev, notes: event.target.value }))}
                      className="rounded-md border border-border bg-background px-3 py-2 text-sm"
                    />
                  </div>
                  <Button
                    variant="primary"
                    onClick={() => handleSubmit(request)}
                    disabled={submitResponse.isPending || !['Sent','PartiallyResponded'].includes(request.status) || !request.items?.length || !request.items.every(item => form.unitPrices[item.id] !== undefined && form.unitPrices[item.id] !== '' && Number.isFinite(Number(form.unitPrices[item.id])) && Number(form.unitPrices[item.id]) >= 0)}
                  >
                    {submitResponse.isPending ? 'Submitting…' : 'Submit Quote'}
                  </Button>
                </div>
                </AsyncState>
              )}
            </div>
          ); })}
          {requests.length === 0 && <p className="text-sm text-body/60">No quotation requests received yet.</p>}
        </div>
        {data?.totalPages > 1 && <div className="mt-6"><Pagination currentPage={page} totalPages={data.totalPages} onPageChange={setPage} /></div>}
      </AsyncState>
    </div>
  );
}
