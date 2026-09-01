import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState, StatusTimeline } from '../../components/ui';
import { useDistricts } from '../../hooks/useDistricts';
import { useShipments, useShipment, useShipmentMutations } from '../../hooks/useShipments';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const serviceLevels = ['Economy', 'Standard', 'Express', 'SameDay'];
const advanceStatuses = ['LabelCreated', 'PickedUp', 'InTransit', 'AtHub', 'OutForDelivery', 'DeliveryFailed', 'Returned'];

const statusTone = {
  Created: 'neutral',
  LabelCreated: 'neutral',
  PickedUp: 'secondary',
  InTransit: 'secondary',
  AtHub: 'secondary',
  OutForDelivery: 'primary',
  Delivered: 'success',
  DeliveryFailed: 'neutral',
  Returned: 'neutral',
  Cancelled: 'neutral',
};

const emptyForm = {
  serviceLevel: 'Standard',
  originContactName: '',
  originPhone: '',
  originAddressLine: '',
  originCity: '',
  originDistrictId: '',
  recipientName: '',
  recipientPhone: '',
  destinationAddressLine: '',
  destinationCity: '',
  destinationDistrictId: '',
  parcelCount: 1,
  totalWeightKg: '',
  shippingCost: '',
  isCashOnDelivery: false,
  codAmount: '',
};

function ShipmentDetail({ id }) {
  const detailQuery = useShipment(id);
  const { updateStatus, markDelivered, cancel } = useShipmentMutations();
  const [statusChoice, setStatusChoice] = useState('');
  const [failureReason, setFailureReason] = useState('');

  const shipment = detailQuery.data;
  if (detailQuery.isLoading) return <p className="py-4 text-sm text-body/60">Loading shipment…</p>;
  if (!shipment) return null;

  const timelineEvents = shipment.events.map((e) => ({
    status: e.toStatus || e.eventType,
    note: e.description || e.locationLabel,
    createdAt: e.occurredAt,
  }));

  const handleAdvance = (event) => {
    event.preventDefault();
    if (!statusChoice) return;
    updateStatus.mutate({
      id,
      payload: {
        status: statusChoice,
        failureReason: statusChoice === 'DeliveryFailed' ? failureReason : undefined,
      },
    });
  };

  const isFinal = ['Delivered', 'Cancelled', 'Returned'].includes(shipment.status);

  return (
    <div className="mt-4 space-y-4 border-t border-border pt-4">
      <div className="grid gap-2 text-xs text-body/60 sm:grid-cols-2">
        <p>Origin: {shipment.originAddressLine}, {shipment.originCity}</p>
        <p>Destination: {shipment.destinationAddressLine}, {shipment.destinationCity}</p>
        {shipment.isCashOnDelivery && (
          <p>COD: ৳{shipment.codAmount?.toLocaleString()} {shipment.codCollected ? '(collected)' : '(pending)'}</p>
        )}
        {shipment.estimatedDeliveryAt && <p>ETA: {new Date(shipment.estimatedDeliveryAt).toLocaleString()}</p>}
      </div>

      {!isFinal && (
        <div className="flex flex-wrap items-end gap-2">
          <form onSubmit={handleAdvance} className="flex flex-wrap items-end gap-2">
            <select value={statusChoice} onChange={(e) => setStatusChoice(e.target.value)} className={inputClass}>
              <option value="">Advance status…</option>
              {advanceStatuses.map((s) => <option key={s} value={s}>{s}</option>)}
            </select>
            {statusChoice === 'DeliveryFailed' && (
              <input placeholder="Failure reason" value={failureReason} onChange={(e) => setFailureReason(e.target.value)} className={inputClass} />
            )}
            <Button type="submit" variant="secondary" size="sm" disabled={updateStatus.isPending || !statusChoice}>Update</Button>
          </form>
          <Button
            variant="primary"
            size="sm"
            disabled={markDelivered.isPending}
            onClick={() => markDelivered.mutate({ id, payload: {} })}
          >
            Mark Delivered
          </Button>
          <button
            type="button"
            onClick={() => cancel.mutate({ id, payload: { reason: 'Cancelled by logistics partner' } })}
            className="text-xs text-danger hover:underline"
          >
            Cancel Shipment
          </button>
        </div>
      )}

      <div>
        <h4 className="mb-2 text-sm font-semibold text-heading">Tracking timeline</h4>
        <StatusTimeline events={timelineEvents} />
      </div>
    </div>
  );
}

export default function Shipments() {
  const [statusFilter, setStatusFilter] = useState('');
  const { data, isLoading, isError, error } = useShipments({ pageSize: 50, status: statusFilter || undefined });
  const districtsQuery = useDistricts();
  const { create } = useShipmentMutations();
  const [showForm, setShowForm] = useState(false);
  const [expandedId, setExpandedId] = useState(null);
  const [form, setForm] = useState(emptyForm);

  const shipments = data?.items || [];

  const handleCreate = (event) => {
    event.preventDefault();
    create.mutate(
      {
        ...form,
        parcelCount: Number(form.parcelCount) || 1,
        totalWeightKg: form.totalWeightKg === '' ? null : Number(form.totalWeightKg),
        shippingCost: form.shippingCost === '' ? null : Number(form.shippingCost),
        codAmount: form.isCashOnDelivery && form.codAmount !== '' ? Number(form.codAmount) : null,
        originDistrictId: form.originDistrictId || null,
        destinationDistrictId: form.destinationDistrictId || null,
      },
      { onSuccess: () => { setShowForm(false); setForm(emptyForm); } },
    );
  };

  return (
    <div>
      <PageHeader
        title="Shipments"
        description="Create and track parcel shipments from pickup to delivery."
        action={<Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'New Shipment'}</Button>}
      />

      <div className="mb-4 flex items-center gap-2">
        <span className="text-xs text-body/60">Filter:</span>
        <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} className={inputClass}>
          <option value="">All statuses</option>
          {['Created', ...advanceStatuses, 'Delivered', 'Cancelled'].map((s) => <option key={s} value={s}>{s}</option>)}
        </select>
      </div>

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <select value={form.serviceLevel} onChange={(e) => setForm((p) => ({ ...p, serviceLevel: e.target.value }))} className={inputClass}>
            {serviceLevels.map((s) => <option key={s} value={s}>{s}</option>)}
          </select>
          <label className="flex items-center gap-2 text-sm text-body/70">
            <input type="checkbox" checked={form.isCashOnDelivery} onChange={(e) => setForm((p) => ({ ...p, isCashOnDelivery: e.target.checked }))} />
            Cash on delivery
          </label>

          <fieldset className="sm:col-span-2">
            <legend className="mb-1 text-xs font-medium text-body/60">Origin</legend>
            <div className="grid gap-3 sm:grid-cols-2">
              <input required placeholder="Contact name" value={form.originContactName} onChange={(e) => setForm((p) => ({ ...p, originContactName: e.target.value }))} className={inputClass} />
              <input required placeholder="Phone" value={form.originPhone} onChange={(e) => setForm((p) => ({ ...p, originPhone: e.target.value }))} className={inputClass} />
              <input required placeholder="Address line" value={form.originAddressLine} onChange={(e) => setForm((p) => ({ ...p, originAddressLine: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
              <input required placeholder="City" value={form.originCity} onChange={(e) => setForm((p) => ({ ...p, originCity: e.target.value }))} className={inputClass} />
              <select value={form.originDistrictId} onChange={(e) => setForm((p) => ({ ...p, originDistrictId: e.target.value }))} className={inputClass}>
                <option value="">District</option>
                {(districtsQuery.data || []).map((d) => <option key={d.id} value={d.id}>{d.name}</option>)}
              </select>
            </div>
          </fieldset>

          <fieldset className="sm:col-span-2">
            <legend className="mb-1 text-xs font-medium text-body/60">Destination</legend>
            <div className="grid gap-3 sm:grid-cols-2">
              <input required placeholder="Recipient name" value={form.recipientName} onChange={(e) => setForm((p) => ({ ...p, recipientName: e.target.value }))} className={inputClass} />
              <input required placeholder="Recipient phone" value={form.recipientPhone} onChange={(e) => setForm((p) => ({ ...p, recipientPhone: e.target.value }))} className={inputClass} />
              <input required placeholder="Address line" value={form.destinationAddressLine} onChange={(e) => setForm((p) => ({ ...p, destinationAddressLine: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
              <input required placeholder="City" value={form.destinationCity} onChange={(e) => setForm((p) => ({ ...p, destinationCity: e.target.value }))} className={inputClass} />
              <select value={form.destinationDistrictId} onChange={(e) => setForm((p) => ({ ...p, destinationDistrictId: e.target.value }))} className={inputClass}>
                <option value="">District</option>
                {(districtsQuery.data || []).map((d) => <option key={d.id} value={d.id}>{d.name}</option>)}
              </select>
            </div>
          </fieldset>

          <input type="number" min="1" placeholder="Parcel count" value={form.parcelCount} onChange={(e) => setForm((p) => ({ ...p, parcelCount: e.target.value }))} className={inputClass} />
          <input type="number" min="0" placeholder="Total weight (kg)" value={form.totalWeightKg} onChange={(e) => setForm((p) => ({ ...p, totalWeightKg: e.target.value }))} className={inputClass} />
          <input type="number" min="0" placeholder="Shipping cost (৳)" value={form.shippingCost} onChange={(e) => setForm((p) => ({ ...p, shippingCost: e.target.value }))} className={inputClass} />
          {form.isCashOnDelivery && (
            <input type="number" min="0" placeholder="COD amount (৳)" value={form.codAmount} onChange={(e) => setForm((p) => ({ ...p, codAmount: e.target.value }))} className={inputClass} />
          )}

          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={create.isPending}>
            {create.isPending ? 'Creating…' : 'Create Shipment'}
          </Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {shipments.map((s) => (
            <div key={s.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{s.trackingNumber}</p>
                  <p className="text-xs text-body/60">
                    {s.recipientName} · {s.destinationCity}{s.destinationDistrictName ? `, ${s.destinationDistrictName}` : ''} · {s.parcelCount} parcel(s)
                    {s.isCashOnDelivery && ' · COD'}
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={statusTone[s.status] || 'neutral'}>{s.status}</Badge>
                  <Button variant="secondary" onClick={() => setExpandedId(expandedId === s.id ? null : s.id)}>
                    {expandedId === s.id ? 'Hide' : 'Track'}
                  </Button>
                </div>
              </div>
              {expandedId === s.id && <ShipmentDetail id={s.id} />}
            </div>
          ))}
          {shipments.length === 0 && <p className="text-sm text-body/60">No shipments yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
