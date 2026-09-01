import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState, StatusTimeline } from '../../components/ui';
import { useLogisticsReturns, useLogisticsReturn, useLogisticsReturnMutations } from '../../hooks/useLogisticsReturns';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const reasons = ['DamagedInTransit', 'DefectiveProduct', 'WrongItem', 'NotAsDescribed', 'CustomerChangedMind', 'DeliveryFailed', 'Undeliverable', 'LateDelivery', 'Other'];
const conditions = ['New', 'LikeNew', 'Used', 'Damaged', 'Defective', 'Unsalvageable'];
const resolutions = ['Refund', 'Replacement', 'Repair', 'StoreCredit', 'NoAction'];
const advanceStatuses = ['InTransit', 'Received', 'UnderInspection', 'Closed'];

const statusTone = {
  Requested: 'neutral', Approved: 'secondary', Rejected: 'neutral', InTransit: 'secondary',
  Received: 'primary', UnderInspection: 'primary', Closed: 'success', Cancelled: 'neutral',
};

const emptyForm = { reason: 'DamagedInTransit', reasonDetail: '', customerName: '', customerPhone: '', pickupAddressLine: '', pickupCity: '' };

function ReturnDetail({ id }) {
  const detailQuery = useLogisticsReturn(id);
  const { approve, reject, updateStatus, recordInspection, restock, recordRefund } = useLogisticsReturnMutations();
  const [statusChoice, setStatusChoice] = useState('');
  const [inspection, setInspection] = useState({ overallCondition: 'Used', summary: '', recommendedResolution: 'Refund' });
  const [refund, setRefund] = useState({ refundAmount: '', refundMethod: '' });

  const ret = detailQuery.data;
  if (detailQuery.isLoading) return <p className="py-4 text-sm text-body/60">Loading…</p>;
  if (!ret) return null;

  const events = ret.events.map((e) => ({ status: e.toStatus || e.type, note: e.note, createdAt: e.createdAt }));

  return (
    <div className="mt-4 space-y-4 border-t border-border pt-4">
      <div className="grid gap-1 text-xs text-body/60 sm:grid-cols-2">
        <p>Customer: {ret.customerName} ({ret.customerPhone})</p>
        <p>Reason: {ret.reason}{ret.reasonDetail ? ` — ${ret.reasonDetail}` : ''}</p>
        {ret.refundAmount != null && <p>Refund: ৳{ret.refundAmount.toLocaleString()} ({ret.resolutionType})</p>}
      </div>

      <div>
        <h4 className="mb-1 text-sm font-semibold text-heading">Items</h4>
        <div className="space-y-1 text-xs text-body/60">
          {ret.items.map((it) => (
            <p key={it.id}>{it.description} × {it.quantity} — {it.condition} / {it.disposition}</p>
          ))}
        </div>
      </div>

      <div className="flex flex-wrap gap-2">
        {ret.status === 'Requested' && (
          <>
            <Button size="sm" variant="primary" disabled={approve.isPending} onClick={() => approve.mutate({ id, payload: {} })}>Approve</Button>
            <Button size="sm" variant="secondary" disabled={reject.isPending} onClick={() => reject.mutate({ id, payload: { reason: 'Does not meet return policy' } })}>Reject</Button>
          </>
        )}
        {!['Requested', 'Rejected', 'Closed', 'Cancelled'].includes(ret.status) && (
          <div className="flex items-center gap-1">
            <select value={statusChoice} onChange={(e) => setStatusChoice(e.target.value)} className={inputClass}>
              <option value="">Advance status…</option>
              {advanceStatuses.map((s) => <option key={s} value={s}>{s}</option>)}
            </select>
            <Button size="sm" variant="secondary" disabled={!statusChoice || updateStatus.isPending} onClick={() => updateStatus.mutate({ id, payload: { status: statusChoice } })}>Update</Button>
          </div>
        )}
      </div>

      {['Received', 'UnderInspection'].includes(ret.status) && (
        <div className="rounded-lg border border-border p-3">
          <p className="mb-2 text-xs font-medium text-body/60">Record inspection</p>
          <div className="flex flex-wrap gap-2">
            <select value={inspection.overallCondition} onChange={(e) => setInspection((p) => ({ ...p, overallCondition: e.target.value }))} className={inputClass}>
              {conditions.map((c) => <option key={c} value={c}>{c}</option>)}
            </select>
            <select value={inspection.recommendedResolution} onChange={(e) => setInspection((p) => ({ ...p, recommendedResolution: e.target.value }))} className={inputClass}>
              {resolutions.map((r) => <option key={r} value={r}>{r}</option>)}
            </select>
            <input placeholder="Summary" value={inspection.summary} onChange={(e) => setInspection((p) => ({ ...p, summary: e.target.value }))} className={`${inputClass} flex-1`} />
            <Button size="sm" variant="secondary" disabled={recordInspection.isPending} onClick={() => recordInspection.mutate({ id, payload: { ...inspection, itemAssessments: [] } })}>
              Save Inspection
            </Button>
          </div>
        </div>
      )}

      {ret.inspections.length > 0 && ret.resolutionType !== 'Refund' && (
        <Button size="sm" variant="secondary" disabled={restock.isPending} onClick={() => restock.mutate({ id, payload: { items: ret.items.map((it) => ({ returnItemId: it.id, restockedQuantity: it.quantityReceived || it.quantity })) } })}>
          Restock All Items
        </Button>
      )}

      {ret.inspections.length > 0 && !ret.refundAmount && (
        <div className="rounded-lg border border-border p-3">
          <p className="mb-2 text-xs font-medium text-body/60">Record refund</p>
          <div className="flex flex-wrap gap-2">
            <input type="number" min="0" placeholder="Amount (৳)" value={refund.refundAmount} onChange={(e) => setRefund((p) => ({ ...p, refundAmount: e.target.value }))} className={`${inputClass} w-32`} />
            <input placeholder="Method" value={refund.refundMethod} onChange={(e) => setRefund((p) => ({ ...p, refundMethod: e.target.value }))} className={inputClass} />
            <Button
              size="sm"
              variant="secondary"
              disabled={!refund.refundAmount || recordRefund.isPending}
              onClick={() => recordRefund.mutate({ id, payload: { refundAmount: Number(refund.refundAmount), refundMethod: refund.refundMethod, resolutionType: 'Refund' } })}
            >
              Save Refund
            </Button>
          </div>
        </div>
      )}

      <div>
        <h4 className="mb-2 text-sm font-semibold text-heading">History</h4>
        <StatusTimeline events={events} />
      </div>
    </div>
  );
}

export default function Returns() {
  const { data, isLoading, isError, error } = useLogisticsReturns({ pageSize: 50 });
  const { create } = useLogisticsReturnMutations();
  const [showForm, setShowForm] = useState(false);
  const [expandedId, setExpandedId] = useState(null);
  const [form, setForm] = useState(emptyForm);
  const [items, setItems] = useState([{ description: '', quantity: 1 }]);

  const returns = data?.items || [];

  const handleCreate = (event) => {
    event.preventDefault();
    create.mutate(
      { ...form, items: items.filter((it) => it.description) },
      { onSuccess: () => { setShowForm(false); setForm(emptyForm); setItems([{ description: '', quantity: 1 }]); } },
    );
  };

  return (
    <div>
      <PageHeader
        title="Returns"
        description="Process customer returns: approve, collect, inspect, restock and refund."
        action={<Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'New Return'}</Button>}
      />

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <select value={form.reason} onChange={(e) => setForm((p) => ({ ...p, reason: e.target.value }))} className={inputClass}>
            {reasons.map((r) => <option key={r} value={r}>{r}</option>)}
          </select>
          <input placeholder="Reason detail" value={form.reasonDetail} onChange={(e) => setForm((p) => ({ ...p, reasonDetail: e.target.value }))} className={inputClass} />
          <input required placeholder="Customer name" value={form.customerName} onChange={(e) => setForm((p) => ({ ...p, customerName: e.target.value }))} className={inputClass} />
          <input required placeholder="Customer phone" value={form.customerPhone} onChange={(e) => setForm((p) => ({ ...p, customerPhone: e.target.value }))} className={inputClass} />
          <input placeholder="Pickup address" value={form.pickupAddressLine} onChange={(e) => setForm((p) => ({ ...p, pickupAddressLine: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <input placeholder="Pickup city" value={form.pickupCity} onChange={(e) => setForm((p) => ({ ...p, pickupCity: e.target.value }))} className={inputClass} />

          <div className="sm:col-span-2">
            <p className="mb-1 text-xs font-medium text-body/60">Items</p>
            {items.map((it, idx) => (
              <div key={idx} className="mb-2 flex gap-2">
                <input placeholder="Description" value={it.description} onChange={(e) => setItems((p) => p.map((x, i) => i === idx ? { ...x, description: e.target.value } : x))} className={`${inputClass} flex-1`} />
                <input type="number" min="1" value={it.quantity} onChange={(e) => setItems((p) => p.map((x, i) => i === idx ? { ...x, quantity: Number(e.target.value) } : x))} className={`${inputClass} w-20`} />
              </div>
            ))}
            <button type="button" onClick={() => setItems((p) => [...p, { description: '', quantity: 1 }])} className="text-xs text-primary hover:underline">+ Add item</button>
          </div>

          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={create.isPending}>
            {create.isPending ? 'Creating…' : 'Create Return'}
          </Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {returns.map((r) => (
            <div key={r.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{r.referenceCode}</p>
                  <p className="text-xs text-body/60">{r.customerName} · {r.reason} · {r.itemCount} item(s)</p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={statusTone[r.status] || 'neutral'}>{r.status}</Badge>
                  <Button variant="secondary" onClick={() => setExpandedId(expandedId === r.id ? null : r.id)}>
                    {expandedId === r.id ? 'Hide' : 'Manage'}
                  </Button>
                </div>
              </div>
              {expandedId === r.id && <ReturnDetail id={r.id} />}
            </div>
          ))}
          {returns.length === 0 && <p className="text-sm text-body/60">No returns yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
