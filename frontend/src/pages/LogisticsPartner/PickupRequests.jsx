import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState, StatusTimeline } from '../../components/ui';
import { useDistricts } from '../../hooks/useDistricts';
import { usePickupRequests, usePickupRequest, usePickupRequestMutations } from '../../hooks/usePickupRequests';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const priorities = ['Standard', 'Express', 'SameDay'];
const advanceStatuses = ['EnRoute', 'Collected', 'Completed', 'Failed'];

const statusTone = { Requested: 'neutral', Scheduled: 'secondary', Assigned: 'secondary', EnRoute: 'primary', Collected: 'primary', Completed: 'success', Failed: 'neutral', Cancelled: 'neutral' };

const emptyForm = {
  priority: 'Standard',
  originContactName: '',
  originPhone: '',
  originAddressLine: '',
  originCity: '',
  originDistrictId: '',
  packageCount: 1,
  totalWeightKg: '',
  requiresColdChain: false,
  isFragile: false,
  isCashOnDelivery: false,
  codAmount: '',
  specialInstructions: '',
};

function PickupDetail({ id }) {
  const detailQuery = usePickupRequest(id);
  const { schedule, assign, updateStatus, cancel } = usePickupRequestMutations();
  const [scheduleAt, setScheduleAt] = useState('');
  const [driver, setDriver] = useState({ assignedDriverName: '', assignedDriverPhone: '', assignedVehicleLabel: '' });
  const [statusChoice, setStatusChoice] = useState('');

  const pickup = detailQuery.data;
  if (detailQuery.isLoading) return <p className="py-4 text-sm text-body/60">Loading…</p>;
  if (!pickup) return null;

  const isFinal = ['Completed', 'Cancelled', 'Failed'].includes(pickup.status);
  const events = pickup.events.map((e) => ({ status: e.toStatus || e.type, note: e.note, createdAt: e.createdAt }));

  return (
    <div className="mt-4 space-y-4 border-t border-border pt-4">
      <div className="grid gap-1 text-xs text-body/60 sm:grid-cols-2">
        <p>Origin: {pickup.originAddressLine}, {pickup.originCity}</p>
        {pickup.destinationCity && <p>Destination: {pickup.destinationAddressLine}, {pickup.destinationCity}</p>}
        {pickup.assignedDriverName && <p>Driver: {pickup.assignedDriverName} ({pickup.assignedDriverPhone})</p>}
        {pickup.scheduledPickupAt && <p>Scheduled: {new Date(pickup.scheduledPickupAt).toLocaleString()}</p>}
      </div>

      {!isFinal && (
        <div className="flex flex-wrap items-end gap-2">
          <label className="flex flex-col gap-1 text-xs text-body/60">
            Schedule pickup
            <div className="flex gap-1">
              <input type="datetime-local" value={scheduleAt} onChange={(e) => setScheduleAt(e.target.value)} className={inputClass} />
              <Button size="sm" variant="secondary" disabled={!scheduleAt || schedule.isPending} onClick={() => schedule.mutate({ id, payload: { scheduledPickupAt: new Date(scheduleAt).toISOString() } })}>Set</Button>
            </div>
          </label>
          <label className="flex flex-col gap-1 text-xs text-body/60">
            Assign driver
            <div className="flex flex-wrap gap-1">
              <input placeholder="Name" value={driver.assignedDriverName} onChange={(e) => setDriver((p) => ({ ...p, assignedDriverName: e.target.value }))} className={`${inputClass} w-28`} />
              <input placeholder="Phone" value={driver.assignedDriverPhone} onChange={(e) => setDriver((p) => ({ ...p, assignedDriverPhone: e.target.value }))} className={`${inputClass} w-28`} />
              <Button size="sm" variant="secondary" disabled={!driver.assignedDriverName || assign.isPending} onClick={() => assign.mutate({ id, payload: driver })}>Assign</Button>
            </div>
          </label>
          <label className="flex flex-col gap-1 text-xs text-body/60">
            Advance status
            <div className="flex gap-1">
              <select value={statusChoice} onChange={(e) => setStatusChoice(e.target.value)} className={inputClass}>
                <option value="">Choose…</option>
                {advanceStatuses.map((s) => <option key={s} value={s}>{s}</option>)}
              </select>
              <Button size="sm" variant="secondary" disabled={!statusChoice || updateStatus.isPending} onClick={() => updateStatus.mutate({ id, payload: { status: statusChoice } })}>Update</Button>
            </div>
          </label>
          <button type="button" onClick={() => cancel.mutate({ id, payload: { reason: 'Cancelled by logistics partner' } })} className="text-xs text-danger hover:underline">
            Cancel Pickup
          </button>
        </div>
      )}

      <div>
        <h4 className="mb-2 text-sm font-semibold text-heading">Items ({pickup.items.length})</h4>
        <div className="space-y-1 text-xs text-body/60">
          {pickup.items.map((it) => (
            <p key={it.id}>{it.description} × {it.quantity}{it.weightKg ? ` · ${it.weightKg}kg` : ''}{it.isFragile ? ' · fragile' : ''}</p>
          ))}
          {pickup.items.length === 0 && <p>No items listed.</p>}
        </div>
      </div>

      <div>
        <h4 className="mb-2 text-sm font-semibold text-heading">History</h4>
        <StatusTimeline events={events} />
      </div>
    </div>
  );
}

export default function PickupRequests() {
  const { data, isLoading, isError, error } = usePickupRequests({ pageSize: 50 });
  const districtsQuery = useDistricts();
  const { create } = usePickupRequestMutations();
  const [showForm, setShowForm] = useState(false);
  const [expandedId, setExpandedId] = useState(null);
  const [form, setForm] = useState(emptyForm);
  const [items, setItems] = useState([{ description: '', quantity: 1 }]);

  const pickups = data?.items || [];

  const handleCreate = (event) => {
    event.preventDefault();
    create.mutate(
      {
        ...form,
        packageCount: Number(form.packageCount) || 1,
        totalWeightKg: form.totalWeightKg === '' ? null : Number(form.totalWeightKg),
        codAmount: form.isCashOnDelivery && form.codAmount !== '' ? Number(form.codAmount) : null,
        originDistrictId: form.originDistrictId || null,
        items: items.filter((it) => it.description),
      },
      { onSuccess: () => { setShowForm(false); setForm(emptyForm); setItems([{ description: '', quantity: 1 }]); } },
    );
  };

  return (
    <div>
      <PageHeader
        title="Pickup Requests"
        description="Schedule and dispatch pickups from producers or customers."
        action={<Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'New Pickup'}</Button>}
      />

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <select value={form.priority} onChange={(e) => setForm((p) => ({ ...p, priority: e.target.value }))} className={inputClass}>
            {priorities.map((p) => <option key={p} value={p}>{p}</option>)}
          </select>
          <input type="number" min="1" placeholder="Package count" value={form.packageCount} onChange={(e) => setForm((p) => ({ ...p, packageCount: e.target.value }))} className={inputClass} />
          <input required placeholder="Contact name" value={form.originContactName} onChange={(e) => setForm((p) => ({ ...p, originContactName: e.target.value }))} className={inputClass} />
          <input required placeholder="Phone" value={form.originPhone} onChange={(e) => setForm((p) => ({ ...p, originPhone: e.target.value }))} className={inputClass} />
          <input required placeholder="Address line" value={form.originAddressLine} onChange={(e) => setForm((p) => ({ ...p, originAddressLine: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <input required placeholder="City" value={form.originCity} onChange={(e) => setForm((p) => ({ ...p, originCity: e.target.value }))} className={inputClass} />
          <select value={form.originDistrictId} onChange={(e) => setForm((p) => ({ ...p, originDistrictId: e.target.value }))} className={inputClass}>
            <option value="">District</option>
            {(districtsQuery.data || []).map((d) => <option key={d.id} value={d.id}>{d.name}</option>)}
          </select>

          <div className="flex flex-wrap gap-4 sm:col-span-2">
            <label className="flex items-center gap-2 text-sm text-body/70">
              <input type="checkbox" checked={form.requiresColdChain} onChange={(e) => setForm((p) => ({ ...p, requiresColdChain: e.target.checked }))} /> Cold chain
            </label>
            <label className="flex items-center gap-2 text-sm text-body/70">
              <input type="checkbox" checked={form.isFragile} onChange={(e) => setForm((p) => ({ ...p, isFragile: e.target.checked }))} /> Fragile
            </label>
            <label className="flex items-center gap-2 text-sm text-body/70">
              <input type="checkbox" checked={form.isCashOnDelivery} onChange={(e) => setForm((p) => ({ ...p, isCashOnDelivery: e.target.checked }))} /> COD
            </label>
          </div>

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

          <textarea rows={2} placeholder="Special instructions" value={form.specialInstructions} onChange={(e) => setForm((p) => ({ ...p, specialInstructions: e.target.value }))} className={`${inputClass} sm:col-span-2`} />

          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={create.isPending}>
            {create.isPending ? 'Creating…' : 'Create Pickup Request'}
          </Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {pickups.map((p) => (
            <div key={p.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{p.referenceCode}</p>
                  <p className="text-xs text-body/60">
                    {p.originCity}{p.originDistrictName ? `, ${p.originDistrictName}` : ''} · {p.packageCount} package(s) · {p.priority}
                    {p.assignedDriverName && ` · ${p.assignedDriverName}`}
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={statusTone[p.status] || 'neutral'}>{p.status}</Badge>
                  <Button variant="secondary" onClick={() => setExpandedId(expandedId === p.id ? null : p.id)}>
                    {expandedId === p.id ? 'Hide' : 'Manage'}
                  </Button>
                </div>
              </div>
              {expandedId === p.id && <PickupDetail id={p.id} />}
            </div>
          ))}
          {pickups.length === 0 && <p className="text-sm text-body/60">No pickup requests yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
