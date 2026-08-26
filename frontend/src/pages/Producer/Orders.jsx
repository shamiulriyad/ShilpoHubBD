import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState, SectionHeader } from '../../components/ui';
import { StatCard } from '../../components/cards';
import {
  useProducerOrderItems,
  useProducerOrderMutations,
  useProducerRevenue,
  useProducerProductPerformance,
} from '../../hooks/useProducerOrders';

const statusTone = { Pending: 'secondary', Accepted: 'primary', Rejected: 'neutral', Processing: 'primary', Shipped: 'success', Delivered: 'success', Cancelled: 'neutral' };
const filters = ['All', 'Pending', 'Accepted', 'Processing', 'Shipped', 'Delivered', 'Rejected'];

export default function Orders() {
  const [status, setStatus] = useState('All');
  const itemsQuery = useProducerOrderItems({ status: status === 'All' ? undefined : status, pageSize: 30 });
  const revenueQuery = useProducerRevenue();
  const performanceQuery = useProducerProductPerformance();
  const { accept, reject, startProcessing, ship, deliver } = useProducerOrderMutations();
  const [shipForm, setShipForm] = useState({});

  const items = itemsQuery.data?.items || [];

  return (
    <div>
      <PageHeader title="Orders & Fulfillment" description="Manage incoming orders and track your sales performance." />

      {revenueQuery.data && (
        <div className="mb-8 grid grid-cols-2 gap-4 lg:grid-cols-4">
          <StatCard label="Total Revenue" value={`৳ ${revenueQuery.data.totalRevenue.toLocaleString()}`} />
          <StatCard label="Total Orders" value={revenueQuery.data.totalOrders} />
          <StatCard label="Avg. Order Value" value={`৳ ${Math.round(revenueQuery.data.averageOrderValue).toLocaleString()}`} />
          <StatCard label="Pending" value={revenueQuery.data.pendingCount} />
        </div>
      )}

      <div className="mb-6 flex flex-wrap gap-2">
        {filters.map((f) => (
          <button
            key={f}
            type="button"
            onClick={() => setStatus(f)}
            className={`rounded-full border px-4 py-1.5 text-sm font-medium transition ${
              status === f ? 'border-primary bg-primary text-surface' : 'border-border bg-surface text-body hover:bg-background'
            }`}
          >
            {f}
          </button>
        ))}
      </div>

      <AsyncState isLoading={itemsQuery.isLoading} isError={itemsQuery.isError} error={itemsQuery.error}>
        <div className="space-y-3">
          {items.map((item) => (
            <div key={item.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{item.productName} × {item.quantity}</p>
                  <p className="text-xs text-body/60">{item.orderNumber} · {item.customerName} · ৳ {item.lineTotal.toLocaleString()}</p>
                </div>
                <Badge tone={statusTone[item.producerStatus] || 'neutral'}>{item.producerStatus}</Badge>
              </div>
              <div className="mt-3 flex flex-wrap items-center gap-2">
                {item.producerStatus === 'Pending' && (
                  <>
                    <Button variant="primary" onClick={() => accept.mutate(item.id)}>Accept</Button>
                    <Button variant="secondary" onClick={() => reject.mutate({ id: item.id, reason: 'Out of stock' })}>Reject</Button>
                  </>
                )}
                {item.producerStatus === 'Accepted' && (
                  <Button variant="primary" onClick={() => startProcessing.mutate(item.id)}>Start Processing</Button>
                )}
                {item.producerStatus === 'Processing' && (
                  <div className="flex flex-wrap items-center gap-2">
                    <input
                      placeholder="Tracking number"
                      value={shipForm[item.id]?.trackingNumber || ''}
                      onChange={(e) => setShipForm((prev) => ({ ...prev, [item.id]: { ...prev[item.id], trackingNumber: e.target.value } }))}
                      className="rounded-md border border-border bg-background px-3 py-2 text-sm"
                    />
                    <input
                      placeholder="Carrier"
                      value={shipForm[item.id]?.carrier || ''}
                      onChange={(e) => setShipForm((prev) => ({ ...prev, [item.id]: { ...prev[item.id], carrier: e.target.value } }))}
                      className="rounded-md border border-border bg-background px-3 py-2 text-sm"
                    />
                    <Button
                      variant="primary"
                      onClick={() => ship.mutate({ id: item.id, payload: shipForm[item.id] || {} })}
                      disabled={!shipForm[item.id]?.trackingNumber || !shipForm[item.id]?.carrier}
                    >
                      Ship
                    </Button>
                  </div>
                )}
                {item.producerStatus === 'Shipped' && (
                  <Button variant="primary" onClick={() => deliver.mutate(item.id)}>Mark Delivered</Button>
                )}
              </div>
            </div>
          ))}
          {items.length === 0 && <p className="text-sm text-body/60">No orders in this status.</p>}
        </div>
      </AsyncState>

      <div className="mt-10">
        <SectionHeader eyebrow="Insights" title="Product Performance" />
        <div className="divide-y divide-border rounded-xl border border-border bg-surface">
          {(performanceQuery.data || []).map((p) => (
            <div key={p.productId} className="flex items-center justify-between p-3 text-sm">
              <span>{p.productName}</span>
              <span className="text-body/60">{p.salesCount} sold · ৳ {p.revenue.toLocaleString()} · ★ {p.averageRating.toFixed(1)}</span>
            </div>
          ))}
          {(performanceQuery.data || []).length === 0 && <p className="p-3 text-sm text-body/60">No sales data yet.</p>}
        </div>
      </div>
    </div>
  );
}
