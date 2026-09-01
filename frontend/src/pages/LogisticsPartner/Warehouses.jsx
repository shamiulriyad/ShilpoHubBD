import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useDistricts } from '../../hooks/useDistricts';
import { useWarehouses, useWarehouse, useWarehouseMutations } from '../../hooks/useWarehouses';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const warehouseTypes = ['Distribution', 'Fulfillment', 'ColdStorage', 'CrossDock', 'Returns', 'Hub'];
const warehouseStatuses = ['Active', 'Inactive', 'Maintenance', 'Closed'];
const zoneTypes = ['Receiving', 'Storage', 'Picking', 'Packing', 'Dispatch', 'Returns', 'ColdStorage', 'Quarantine', 'Staging'];
const binTypes = ['Shelf', 'Rack', 'Pallet', 'Floor', 'Bulk', 'Bin', 'ColdUnit'];

const statusTone = { Active: 'success', Inactive: 'neutral', Maintenance: 'secondary', Closed: 'neutral' };

const emptyCreateForm = {
  name: '',
  type: 'Distribution',
  addressLine: '',
  city: '',
  districtId: '',
  postalCode: '',
  contactPersonName: '',
  contactPhone: '',
  totalCapacityUnits: 0,
  hasColdChain: false,
  handlesHazardous: false,
  handlesReturns: false,
};

function WarehouseDetail({ id }) {
  const detailQuery = useWarehouse(id);
  const { addZone, removeZone, addBin, removeBin } = useWarehouseMutations();
  const [zoneForm, setZoneForm] = useState({ code: '', name: '', type: 'Storage', capacityUnits: 0 });
  const [binForm, setBinForm] = useState({ code: '', label: '', type: 'Shelf', capacityUnits: 0, warehouseZoneId: '' });

  const warehouse = detailQuery.data;

  const handleAddZone = (event) => {
    event.preventDefault();
    if (!zoneForm.code || !zoneForm.name) return;
    addZone.mutate(
      { id, payload: { ...zoneForm, capacityUnits: Number(zoneForm.capacityUnits) || 0, isColdChain: false, isActive: true } },
      { onSuccess: () => setZoneForm({ code: '', name: '', type: 'Storage', capacityUnits: 0 }) },
    );
  };

  const handleAddBin = (event) => {
    event.preventDefault();
    if (!binForm.code) return;
    addBin.mutate(
      {
        id,
        payload: {
          ...binForm,
          capacityUnits: Number(binForm.capacityUnits) || 0,
          warehouseZoneId: binForm.warehouseZoneId || null,
          isPickable: true,
          isActive: true,
        },
      },
      { onSuccess: () => setBinForm({ code: '', label: '', type: 'Shelf', capacityUnits: 0, warehouseZoneId: '' }) },
    );
  };

  if (detailQuery.isLoading) return <p className="py-4 text-sm text-body/60">Loading warehouse…</p>;
  if (!warehouse) return null;

  return (
    <div className="mt-4 grid gap-4 border-t border-border pt-4 sm:grid-cols-2">
      <div>
        <h4 className="mb-2 text-sm font-semibold text-heading">Zones ({warehouse.zones.length})</h4>
        <div className="mb-3 space-y-2">
          {warehouse.zones.map((zone) => (
            <div key={zone.id} className="flex items-center justify-between rounded-lg border border-border px-3 py-2 text-xs">
              <span>{zone.code} — {zone.name} ({zone.type}) · {zone.capacityUnits} units</span>
              <button
                type="button"
                onClick={() => removeZone.mutate({ id, zoneId: zone.id })}
                className="text-danger hover:underline"
              >
                Remove
              </button>
            </div>
          ))}
          {warehouse.zones.length === 0 && <p className="text-xs text-body/50">No zones yet.</p>}
        </div>
        <form onSubmit={handleAddZone} className="flex flex-wrap gap-2">
          <input placeholder="Code" value={zoneForm.code} onChange={(e) => setZoneForm((p) => ({ ...p, code: e.target.value }))} className={`${inputClass} w-24`} />
          <input placeholder="Name" value={zoneForm.name} onChange={(e) => setZoneForm((p) => ({ ...p, name: e.target.value }))} className={`${inputClass} w-32`} />
          <select value={zoneForm.type} onChange={(e) => setZoneForm((p) => ({ ...p, type: e.target.value }))} className={inputClass}>
            {zoneTypes.map((t) => <option key={t} value={t}>{t}</option>)}
          </select>
          <input type="number" placeholder="Capacity" value={zoneForm.capacityUnits} onChange={(e) => setZoneForm((p) => ({ ...p, capacityUnits: e.target.value }))} className={`${inputClass} w-24`} />
          <Button type="submit" variant="secondary" size="sm" disabled={addZone.isPending}>Add Zone</Button>
        </form>
      </div>

      <div>
        <h4 className="mb-2 text-sm font-semibold text-heading">Bins ({warehouse.bins.length})</h4>
        <div className="mb-3 space-y-2">
          {warehouse.bins.map((bin) => (
            <div key={bin.id} className="flex items-center justify-between rounded-lg border border-border px-3 py-2 text-xs">
              <span>{bin.code} {bin.zoneCode ? `(${bin.zoneCode})` : ''} — {bin.type} · {bin.occupiedUnits}/{bin.capacityUnits}</span>
              <button
                type="button"
                onClick={() => removeBin.mutate({ id, binId: bin.id })}
                className="text-danger hover:underline"
              >
                Remove
              </button>
            </div>
          ))}
          {warehouse.bins.length === 0 && <p className="text-xs text-body/50">No bins yet.</p>}
        </div>
        <form onSubmit={handleAddBin} className="flex flex-wrap gap-2">
          <input placeholder="Code" value={binForm.code} onChange={(e) => setBinForm((p) => ({ ...p, code: e.target.value }))} className={`${inputClass} w-24`} />
          <select value={binForm.warehouseZoneId} onChange={(e) => setBinForm((p) => ({ ...p, warehouseZoneId: e.target.value }))} className={inputClass}>
            <option value="">No zone</option>
            {warehouse.zones.map((z) => <option key={z.id} value={z.id}>{z.code}</option>)}
          </select>
          <select value={binForm.type} onChange={(e) => setBinForm((p) => ({ ...p, type: e.target.value }))} className={inputClass}>
            {binTypes.map((t) => <option key={t} value={t}>{t}</option>)}
          </select>
          <input type="number" placeholder="Capacity" value={binForm.capacityUnits} onChange={(e) => setBinForm((p) => ({ ...p, capacityUnits: e.target.value }))} className={`${inputClass} w-24`} />
          <Button type="submit" variant="secondary" size="sm" disabled={addBin.isPending}>Add Bin</Button>
        </form>
      </div>
    </div>
  );
}

export default function Warehouses() {
  const { data, isLoading, isError, error } = useWarehouses({ pageSize: 50 });
  const districtsQuery = useDistricts();
  const { create, remove } = useWarehouseMutations();
  const [showForm, setShowForm] = useState(false);
  const [expandedId, setExpandedId] = useState(null);
  const [form, setForm] = useState(emptyCreateForm);

  const warehouses = data?.items || [];

  const handleCreate = (event) => {
    event.preventDefault();
    create.mutate(
      { ...form, totalCapacityUnits: Number(form.totalCapacityUnits) || 0, districtId: form.districtId || null },
      { onSuccess: () => { setShowForm(false); setForm(emptyCreateForm); } },
    );
  };

  return (
    <div>
      <PageHeader
        title="Warehouses"
        description="Manage your storage facilities, zones and bins."
        action={<Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'New Warehouse'}</Button>}
      />

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <input required placeholder="Warehouse name" value={form.name} onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))} className={inputClass} />
          <select value={form.type} onChange={(e) => setForm((p) => ({ ...p, type: e.target.value }))} className={inputClass}>
            {warehouseTypes.map((t) => <option key={t} value={t}>{t}</option>)}
          </select>
          <input required placeholder="Address line" value={form.addressLine} onChange={(e) => setForm((p) => ({ ...p, addressLine: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <input required placeholder="City" value={form.city} onChange={(e) => setForm((p) => ({ ...p, city: e.target.value }))} className={inputClass} />
          <select value={form.districtId} onChange={(e) => setForm((p) => ({ ...p, districtId: e.target.value }))} className={inputClass}>
            <option value="">Select district</option>
            {(districtsQuery.data || []).map((d) => <option key={d.id} value={d.id}>{d.name}</option>)}
          </select>
          <input placeholder="Postal code" value={form.postalCode} onChange={(e) => setForm((p) => ({ ...p, postalCode: e.target.value }))} className={inputClass} />
          <input type="number" min="0" placeholder="Total capacity units" value={form.totalCapacityUnits} onChange={(e) => setForm((p) => ({ ...p, totalCapacityUnits: e.target.value }))} className={inputClass} />
          <input placeholder="Contact person" value={form.contactPersonName} onChange={(e) => setForm((p) => ({ ...p, contactPersonName: e.target.value }))} className={inputClass} />
          <input placeholder="Contact phone" value={form.contactPhone} onChange={(e) => setForm((p) => ({ ...p, contactPhone: e.target.value }))} className={inputClass} />
          <div className="flex flex-wrap gap-4 sm:col-span-2">
            <label className="flex items-center gap-2 text-sm text-body/70">
              <input type="checkbox" checked={form.hasColdChain} onChange={(e) => setForm((p) => ({ ...p, hasColdChain: e.target.checked }))} />
              Cold chain
            </label>
            <label className="flex items-center gap-2 text-sm text-body/70">
              <input type="checkbox" checked={form.handlesHazardous} onChange={(e) => setForm((p) => ({ ...p, handlesHazardous: e.target.checked }))} />
              Hazardous handling
            </label>
            <label className="flex items-center gap-2 text-sm text-body/70">
              <input type="checkbox" checked={form.handlesReturns} onChange={(e) => setForm((p) => ({ ...p, handlesReturns: e.target.checked }))} />
              Handles returns
            </label>
          </div>
          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={create.isPending}>
            {create.isPending ? 'Creating…' : 'Create Warehouse'}
          </Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {warehouses.map((wh) => (
            <div key={wh.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{wh.name} <span className="font-normal text-body/50">({wh.code})</span></p>
                  <p className="text-xs text-body/60">
                    {wh.type} · {wh.city}{wh.districtName ? `, ${wh.districtName}` : ''} · {wh.usedCapacityUnits}/{wh.totalCapacityUnits} units
                    {wh.hasColdChain && ' · ❄️ cold chain'}
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={statusTone[wh.status] || 'neutral'}>{wh.status}</Badge>
                  <Button variant="secondary" onClick={() => setExpandedId(expandedId === wh.id ? null : wh.id)}>
                    {expandedId === wh.id ? 'Hide' : 'Manage'}
                  </Button>
                  <button
                    type="button"
                    onClick={() => remove.mutate(wh.id)}
                    className="text-xs text-danger hover:underline"
                  >
                    Delete
                  </button>
                </div>
              </div>
              {expandedId === wh.id && <WarehouseDetail id={wh.id} />}
            </div>
          ))}
          {warehouses.length === 0 && <p className="text-sm text-body/60">No warehouses yet. Create your first one above.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
