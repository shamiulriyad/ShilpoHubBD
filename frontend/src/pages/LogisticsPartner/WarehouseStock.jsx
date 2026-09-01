import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState, StatusTimeline } from '../../components/ui';
import { useWarehouses } from '../../hooks/useWarehouses';
import { useWarehouseStockItems, useWarehouseStockItem, useWarehouseStockMutations } from '../../hooks/useWarehouseStock';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';

const emptyReceiveForm = {
  warehouseId: '',
  sku: '',
  description: '',
  unitOfMeasure: 'unit',
  quantity: 1,
  batchNumber: '',
  unitValue: '',
};

function StockDetail({ id }) {
  const detailQuery = useWarehouseStockItem(id);
  const { issue, adjust, reserve, release } = useWarehouseStockMutations();
  const [qty, setQty] = useState({ issue: '', adjust: '', reserve: '', release: '' });

  const item = detailQuery.data;
  if (detailQuery.isLoading) return <p className="py-4 text-sm text-body/60">Loading…</p>;
  if (!item) return null;

  const events = item.movements.map((m) => ({
    status: `${m.type} · ${m.quantity} → on hand ${m.quantityOnHandAfter}`,
    note: m.reason || m.note,
    createdAt: m.occurredAt,
  }));

  return (
    <div className="mt-4 space-y-4 border-t border-border pt-4">
      <div className="flex flex-wrap gap-4 text-xs text-body/60">
        <span>On hand: <strong className="text-body">{item.quantityOnHand}</strong></span>
        <span>Reserved: <strong className="text-body">{item.quantityReserved}</strong></span>
        <span>Available: <strong className="text-body">{item.quantityAvailable}</strong></span>
        {item.expiryDate && <span>Expires: {new Date(item.expiryDate).toLocaleDateString()}</span>}
      </div>

      <div className="flex flex-wrap items-end gap-3">
        <label className="flex flex-col gap-1 text-xs text-body/60">
          Issue qty
          <div className="flex gap-1">
            <input type="number" min="1" value={qty.issue} onChange={(e) => setQty((p) => ({ ...p, issue: e.target.value }))} className={`${inputClass} w-20`} />
            <Button size="sm" variant="secondary" disabled={!qty.issue || issue.isPending} onClick={() => issue.mutate({ id, payload: { quantity: Number(qty.issue) } })}>Issue</Button>
          </div>
        </label>
        <label className="flex flex-col gap-1 text-xs text-body/60">
          Reserve qty
          <div className="flex gap-1">
            <input type="number" min="1" value={qty.reserve} onChange={(e) => setQty((p) => ({ ...p, reserve: e.target.value }))} className={`${inputClass} w-20`} />
            <Button size="sm" variant="secondary" disabled={!qty.reserve || reserve.isPending} onClick={() => reserve.mutate({ id, payload: { quantity: Number(qty.reserve) } })}>Reserve</Button>
          </div>
        </label>
        <label className="flex flex-col gap-1 text-xs text-body/60">
          Release qty
          <div className="flex gap-1">
            <input type="number" min="1" value={qty.release} onChange={(e) => setQty((p) => ({ ...p, release: e.target.value }))} className={`${inputClass} w-20`} />
            <Button size="sm" variant="secondary" disabled={!qty.release || release.isPending} onClick={() => release.mutate({ id, payload: { quantity: Number(qty.release) } })}>Release</Button>
          </div>
        </label>
        <label className="flex flex-col gap-1 text-xs text-body/60">
          Adjust to
          <div className="flex gap-1">
            <input type="number" min="0" value={qty.adjust} onChange={(e) => setQty((p) => ({ ...p, adjust: e.target.value }))} className={`${inputClass} w-20`} />
            <Button
              size="sm"
              variant="secondary"
              disabled={qty.adjust === '' || adjust.isPending}
              onClick={() => adjust.mutate({ id, payload: { newQuantityOnHand: Number(qty.adjust), reason: 'Manual adjustment' } })}
            >
              Adjust
            </Button>
          </div>
        </label>
      </div>

      <div>
        <h4 className="mb-2 text-sm font-semibold text-heading">Movement history</h4>
        <StatusTimeline events={events} />
      </div>
    </div>
  );
}

export default function WarehouseStock() {
  const [filters, setFilters] = useState({ warehouseId: '', lowStock: false, expiringSoon: false, search: '' });
  const { data, isLoading, isError, error } = useWarehouseStockItems({
    pageSize: 50,
    warehouseId: filters.warehouseId || undefined,
    lowStock: filters.lowStock || undefined,
    expiringSoon: filters.expiringSoon || undefined,
    search: filters.search || undefined,
  });
  const warehousesQuery = useWarehouses({ pageSize: 100 });
  const { receive, remove } = useWarehouseStockMutations();
  const [showForm, setShowForm] = useState(false);
  const [expandedId, setExpandedId] = useState(null);
  const [form, setForm] = useState(emptyReceiveForm);

  const items = data?.items || [];
  const warehouses = warehousesQuery.data?.items || [];

  const handleReceive = (event) => {
    event.preventDefault();
    receive.mutate(
      { ...form, quantity: Number(form.quantity) || 1, unitValue: form.unitValue === '' ? null : Number(form.unitValue) },
      { onSuccess: () => { setShowForm(false); setForm(emptyReceiveForm); } },
    );
  };

  return (
    <div>
      <PageHeader
        title="Warehouse Stock"
        description="Track inventory across your warehouses: receive, issue, transfer, adjust and reserve stock."
        action={<Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'Receive Stock'}</Button>}
      />

      <div className="mb-4 flex flex-wrap items-center gap-3">
        <select value={filters.warehouseId} onChange={(e) => setFilters((p) => ({ ...p, warehouseId: e.target.value }))} className={inputClass}>
          <option value="">All warehouses</option>
          {warehouses.map((w) => <option key={w.id} value={w.id}>{w.name}</option>)}
        </select>
        <input placeholder="Search SKU/description" value={filters.search} onChange={(e) => setFilters((p) => ({ ...p, search: e.target.value }))} className={inputClass} />
        <label className="flex items-center gap-2 text-sm text-body/70">
          <input type="checkbox" checked={filters.lowStock} onChange={(e) => setFilters((p) => ({ ...p, lowStock: e.target.checked }))} />
          Low stock
        </label>
        <label className="flex items-center gap-2 text-sm text-body/70">
          <input type="checkbox" checked={filters.expiringSoon} onChange={(e) => setFilters((p) => ({ ...p, expiringSoon: e.target.checked }))} />
          Expiring soon
        </label>
      </div>

      {showForm && (
        <form onSubmit={handleReceive} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <select required value={form.warehouseId} onChange={(e) => setForm((p) => ({ ...p, warehouseId: e.target.value }))} className={inputClass}>
            <option value="">Select warehouse</option>
            {warehouses.map((w) => <option key={w.id} value={w.id}>{w.name}</option>)}
          </select>
          <input required placeholder="SKU" value={form.sku} onChange={(e) => setForm((p) => ({ ...p, sku: e.target.value }))} className={inputClass} />
          <input required placeholder="Description" value={form.description} onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <input placeholder="Unit of measure" value={form.unitOfMeasure} onChange={(e) => setForm((p) => ({ ...p, unitOfMeasure: e.target.value }))} className={inputClass} />
          <input required type="number" min="1" placeholder="Quantity" value={form.quantity} onChange={(e) => setForm((p) => ({ ...p, quantity: e.target.value }))} className={inputClass} />
          <input placeholder="Batch number" value={form.batchNumber} onChange={(e) => setForm((p) => ({ ...p, batchNumber: e.target.value }))} className={inputClass} />
          <input type="number" min="0" placeholder="Unit value (৳)" value={form.unitValue} onChange={(e) => setForm((p) => ({ ...p, unitValue: e.target.value }))} className={inputClass} />
          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={receive.isPending}>
            {receive.isPending ? 'Receiving…' : 'Receive Stock'}
          </Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {items.map((item) => (
            <div key={item.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{item.sku} — {item.description}</p>
                  <p className="text-xs text-body/60">
                    {item.binCode ? `Bin ${item.binCode} · ` : ''}{item.quantityAvailable} available / {item.quantityOnHand} on hand ({item.unitOfMeasure})
                    {item.batchNumber && ` · Batch ${item.batchNumber}`}
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge>{item.status}</Badge>
                  <Button variant="secondary" onClick={() => setExpandedId(expandedId === item.id ? null : item.id)}>
                    {expandedId === item.id ? 'Hide' : 'Manage'}
                  </Button>
                  <button type="button" onClick={() => remove.mutate(item.id)} className="text-xs text-danger hover:underline">Delete</button>
                </div>
              </div>
              {expandedId === item.id && <StockDetail id={item.id} />}
            </div>
          ))}
          {items.length === 0 && <p className="text-sm text-body/60">No stock items found.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
