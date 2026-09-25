import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useDistricts } from '../../hooks/useDistricts';
import { useDeliveryRoutes, useDeliveryRoute, useDeliveryRouteMutations } from '../../hooks/useDeliveryRoutes';

import { confirmAction } from '../../lib/confirm';
const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const stopTypes = ['Pickup', 'Delivery', 'Transfer', 'Waypoint'];

const statusTone = {
  Draft: 'neutral', Planned: 'secondary', Assigned: 'secondary', Dispatched: 'primary',
  InProgress: 'primary', Completed: 'success', Cancelled: 'neutral',
};
const stopStatusTone = { Pending: 'neutral', Arrived: 'secondary', Completed: 'success', Skipped: 'neutral', Failed: 'neutral' };

function RouteDetail({ id }) {
  const detailQuery = useDeliveryRoute(id);
  const {
    addStop, removeStop, optimize, assign, dispatch, start, complete, cancel, arriveStop, completeStop, skipStop, failStop,
  } = useDeliveryRouteMutations();
  const [driver, setDriver] = useState({ assignedDriverName: '', assignedDriverPhone: '', assignedVehicleLabel: '' });
  const [stopForm, setStopForm] = useState({ stopType: 'Delivery', addressLine: '', city: '', contactName: '' });

  const route = detailQuery.data;
  if (detailQuery.isLoading) return <p className="py-4 text-sm text-body/60">Loading…</p>;
  if (detailQuery.isError) return <p role="alert" className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">Unable to load this record. It may have been removed or you may not have access.</p>;
  if (!route) return <p className="py-4 text-sm text-body/60">This record is unavailable.</p>;

  const isFinal = ['Completed', 'Cancelled'].includes(route.status);

  const handleAddStop = (event) => {
    event.preventDefault();
    if (!stopForm.addressLine || !stopForm.city) return;
    addStop.mutate({ id, payload: { ...stopForm, packageCount: 1 } }, { onSuccess: () => setStopForm({ stopType: 'Delivery', addressLine: '', city: '', contactName: '' }) });
  };

  return (
    <div className="mt-4 space-y-4 border-t border-border pt-4">
      <div className="flex flex-wrap gap-4 text-xs text-body/60">
        <span>{route.completedStops}/{route.totalStops} stops complete</span>
        {route.totalDistanceKm != null && <span>{route.totalDistanceKm.toFixed?.(1) ?? route.totalDistanceKm} km</span>}
        {route.assignedDriverName && <span>Driver: {route.assignedDriverName}</span>}
      </div>

      {!isFinal && (
        <div className="flex flex-wrap items-end gap-2">
          {route.status === 'Draft' || route.status === 'Planned' ? (
            <>
              <div className="flex flex-wrap gap-1">
                <input aria-label="Driver name" placeholder="Driver name" value={driver.assignedDriverName} onChange={(e) => setDriver((p) => ({ ...p, assignedDriverName: e.target.value }))} className={`${inputClass} w-28`} />
                <input aria-label="Phone" placeholder="Phone" value={driver.assignedDriverPhone} onChange={(e) => setDriver((p) => ({ ...p, assignedDriverPhone: e.target.value }))} className={`${inputClass} w-28`} />
                <input aria-label="Vehicle" placeholder="Vehicle" value={driver.assignedVehicleLabel} onChange={(e) => setDriver((p) => ({ ...p, assignedVehicleLabel: e.target.value }))} className={`${inputClass} w-24`} />
                <Button size="sm" variant="secondary" disabled={!driver.assignedDriverName || assign.isPending} onClick={() => assign.mutate({ id, payload: driver })}>Assign</Button>
              </div>
              <Button size="sm" variant="secondary" disabled={optimize.isPending || route.stops.length < 2} onClick={() => optimize.mutate({ id, payload: {} })}>
                {optimize.isPending ? 'Optimizing…' : 'AI Optimize'}
              </Button>
            </>
          ) : null}
          {route.status === 'Assigned' && (
            <Button size="sm" variant="primary" disabled={dispatch.isPending} onClick={() => dispatch.mutate(id)}>Dispatch</Button>
          )}
          {route.status === 'Dispatched' && (
            <Button size="sm" variant="primary" disabled={start.isPending} onClick={() => start.mutate(id)}>Start Route</Button>
          )}
          {route.status === 'InProgress' && (
            <Button size="sm" variant="primary" disabled={complete.isPending} onClick={() => complete.mutate(id)}>Complete Route</Button>
          )}
          <button type="button" onClick={async () => { if (await confirmAction('Cancel this? It may not be possible to undo.', { confirmLabel: 'Yes, cancel it' })) cancel.mutate({ id, payload: { reason: 'Cancelled by logistics partner' } }); }} className="text-xs text-danger hover:underline">
            Cancel Route
          </button>
        </div>
      )}

      <div>
        <h4 className="mb-2 text-sm font-semibold text-heading">Stops</h4>
        <div className="space-y-2">
          {route.stops.map((stop) => (
            <div key={stop.id} className="flex flex-wrap items-center justify-between gap-2 rounded-lg border border-border px-3 py-2 text-xs">
              <span>#{stop.sequence} {stop.stopType} — {stop.addressLine}, {stop.city}{stop.contactName ? ` (${stop.contactName})` : ''}</span>
              <div className="flex items-center gap-2">
                <Badge tone={stopStatusTone[stop.status] || 'neutral'}>{stop.status}</Badge>
                {!isFinal && stop.status === 'Pending' && route.status === 'InProgress' && (
                  <button type="button" onClick={() => arriveStop.mutate({ id, stopId: stop.id })} className="text-primary hover:underline">Arrive</button>
                )}
                {!isFinal && stop.status === 'Arrived' && (
                  <button type="button" onClick={() => completeStop.mutate({ id, stopId: stop.id, payload: {} })} className="text-success hover:underline">Complete</button>
                )}
                {!isFinal && ['Pending', 'Arrived'].includes(stop.status) && (
                  <button type="button" onClick={() => failStop.mutate({ id, stopId: stop.id, payload: { failureReason: 'Recipient unavailable' } })} className="text-danger hover:underline">Fail</button>
                )}
                {!isFinal && route.status !== 'InProgress' && (
                  <button type="button" onClick={async () => { if (await confirmAction('Remove this? This cannot be undone.', { confirmLabel: 'Yes, remove' })) removeStop.mutate({ id, stopId: stop.id }); }} className="text-danger hover:underline">Remove</button>
                )}
              </div>
            </div>
          ))}
          {route.stops.length === 0 && <p className="text-xs text-body/50">No stops yet.</p>}
        </div>

        {(route.status === 'Draft' || route.status === 'Planned') && (
          <form onSubmit={handleAddStop} className="mt-3 flex flex-wrap gap-2">
            <select aria-label="Stop Type" value={stopForm.stopType} onChange={(e) => setStopForm((p) => ({ ...p, stopType: e.target.value }))} className={inputClass}>
              {stopTypes.map((t) => <option key={t} value={t}>{t}</option>)}
            </select>
            <input aria-label="Contact name" placeholder="Contact name" value={stopForm.contactName} onChange={(e) => setStopForm((p) => ({ ...p, contactName: e.target.value }))} className={`${inputClass} w-32`} />
            <input aria-label="Address line" placeholder="Address line" value={stopForm.addressLine} onChange={(e) => setStopForm((p) => ({ ...p, addressLine: e.target.value }))} className={`${inputClass} flex-1`} />
            <input aria-label="City" placeholder="City" value={stopForm.city} onChange={(e) => setStopForm((p) => ({ ...p, city: e.target.value }))} className={`${inputClass} w-28`} />
            <Button type="submit" variant="secondary" size="sm" disabled={addStop.isPending}>Add Stop</Button>
          </form>
        )}
      </div>
    </div>
  );
}

export default function DeliveryRoutes() {
  const { data, isLoading, isError, error } = useDeliveryRoutes({ pageSize: 50 });
  const districtsQuery = useDistricts();
  const { create } = useDeliveryRouteMutations();
  const [showForm, setShowForm] = useState(false);
  const [expandedId, setExpandedId] = useState(null);
  const [form, setForm] = useState({ name: '', scheduledDate: '', originDistrictId: '', startLocationLabel: '', endLocationLabel: '', notes: '', vehicleCapacityKg: '' });

  const routes = data?.items || [];

  const handleCreate = (event) => {
    event.preventDefault();
    create.mutate(
      {
        name: form.name.trim(),
        scheduledDate: form.scheduledDate || null,
        originDistrictId: form.originDistrictId || null,
        startLocationLabel: form.startLocationLabel.trim() || null,
        endLocationLabel: form.endLocationLabel.trim() || null,
        notes: form.notes.trim() || null,
        vehicleCapacityKg: form.vehicleCapacityKg ? Number(form.vehicleCapacityKg) : null,
        stops: [],
      },
      { onSuccess: () => { setShowForm(false); setForm({ name: '', scheduledDate: '', originDistrictId: '', startLocationLabel: '', endLocationLabel: '', notes: '', vehicleCapacityKg: '' }); } },
    );
  };

  return (
    <div>
      <PageHeader
        title="Delivery Routes"
        description="Plan routes, add stops, and dispatch drivers for last-mile delivery."
        action={<Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'New Route'}</Button>}
      />

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <input aria-label="Route name" required placeholder="Route name" value={form.name} onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))} className={inputClass} />
          <input aria-label="Scheduled Date" type="date" value={form.scheduledDate} onChange={(e) => setForm((p) => ({ ...p, scheduledDate: e.target.value }))} className={inputClass} />
          <select aria-label="Origin District Id" value={form.originDistrictId} onChange={(e) => setForm((p) => ({ ...p, originDistrictId: e.target.value }))} className={inputClass}>
            <option value="">Origin district</option>
            {(districtsQuery.data || []).map((d) => <option key={d.id} value={d.id}>{d.name}</option>)}
          </select>
          <input aria-label="From" required placeholder="From (start point, e.g. Dhaka - Tejgaon)" value={form.startLocationLabel} onChange={(e) => setForm((p) => ({ ...p, startLocationLabel: e.target.value }))} className={inputClass} />
          <input aria-label="To" required placeholder="To (end point, e.g. Chattogram - Agrabad)" value={form.endLocationLabel} onChange={(e) => setForm((p) => ({ ...p, endLocationLabel: e.target.value }))} className={inputClass} />
          <input aria-label="Roads and districts on the way" placeholder="Roads / districts on the way (e.g. Dhaka-Chattogram highway via Comilla)" value={form.notes} onChange={(e) => setForm((p) => ({ ...p, notes: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <input aria-label="Vehicle capacity (kg)" type="number" min="0" step="any" placeholder="Vehicle capacity (kg)" value={form.vehicleCapacityKg} onChange={(e) => setForm((p) => ({ ...p, vehicleCapacityKg: e.target.value }))} className={inputClass} />
          <p className="self-center text-xs text-body/60">Producers see this route (from, to, roads, date) and pick the one they need when handing over a parcel.</p>
          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={create.isPending}>
            {create.isPending ? 'Creating…' : 'Create Route'}
          </Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {routes.map((r) => (
            <div key={r.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{r.name} <span className="font-normal text-body/50">({r.routeCode})</span></p>
                  <p className="text-xs text-body/60">
                    {[r.startLocationLabel, r.endLocationLabel].filter(Boolean).join(' → ') || 'No start/end set'}{r.originDistrictName ? ` · ${r.originDistrictName}` : ''} · {r.completedStops}/{r.totalStops} stops
                    {r.assignedDriverName && ` · ${r.assignedDriverName}`}
                    {r.scheduledDate && ` · ${new Date(r.scheduledDate).toLocaleDateString()}`}
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={statusTone[r.status] || 'neutral'}>{r.status}</Badge>
                  <Button variant="secondary" onClick={() => setExpandedId(expandedId === r.id ? null : r.id)}>
                    {expandedId === r.id ? 'Hide' : 'Manage'}
                  </Button>
                </div>
              </div>
              {expandedId === r.id && <RouteDetail id={r.id} />}
            </div>
          ))}
          {routes.length === 0 && <p className="text-sm text-body/60">No delivery routes yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
