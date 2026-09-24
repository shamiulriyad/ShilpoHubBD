import MutationFeedback from '../../components/ui/MutationFeedback';
import ProducerInsights from './ProducerInsights';
import { useLogisticsDirectory } from '../../hooks/useLogisticsDirectory';
import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState, SectionHeader, Pagination, QueryStatusBanner } from '../../components/ui';
import { StatCard } from '../../components/cards';
import {
  useProducerOrderItems,
  useProducerOrderMutations,
  useProducerRevenue,
  useProducerProductPerformance,
} from '../../hooks/useProducerOrders';

const statusTone = { Pending: 'secondary', Accepted: 'primary', Rejected: 'neutral', Processing: 'primary', Shipped: 'success', Delivered: 'success', Cancelled: 'neutral' };
const filters = ['All', 'Pending', 'Accepted', 'Processing', 'Shipped', 'Delivered', 'Rejected'];

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';

// Shipping an item: either hand it to a verified logistics partner (they get the shipment and the
// tracking number is generated) or ship it yourself and enter a tracking number and carrier.
function ShipControls({ item, partners, form, onChange, onShip, shipping }) {
  const viaPartner = Boolean(form.logisticsPartnerProfileId);
  const manualOk = form.trackingNumber?.trim() && form.carrier?.trim();
  return (
    <div className="flex flex-wrap items-center gap-2">
      <select
        aria-label="Delivery partner"
        value={form.logisticsPartnerProfileId || ''}
        onChange={(e) => onChange({ logisticsPartnerProfileId: e.target.value })}
        className={inputClass}
      >
        <option value="">I will ship it myself</option>
        {partners.map((p) => (
          <option key={p.profileId} value={p.profileId}>Hand over to {p.companyName}{p.baseCity ? ` (${p.baseCity})` : ''}</option>
        ))}
      </select>
      {!viaPartner && (
        <>
          <input aria-label="Tracking number" placeholder="Tracking number" value={form.trackingNumber || ''} onChange={(e) => onChange({ trackingNumber: e.target.value })} className={inputClass} />
          <input aria-label="Carrier" placeholder="Carrier" value={form.carrier || ''} onChange={(e) => onChange({ carrier: e.target.value })} className={inputClass} />
        </>
      )}
      <Button
        variant="primary"
        onClick={() => onShip(viaPartner ? { logisticsPartnerProfileId: form.logisticsPartnerProfileId } : { trackingNumber: form.trackingNumber, carrier: form.carrier })}
        disabled={shipping || (!viaPartner && !manualOk)}
      >
        {viaPartner ? 'Hand over & ship' : 'Ship'}
      </Button>
      {viaPartner && <p className="basis-full text-xs text-body/60">The partner receives this order as a shipment and will contact you to arrange the pickup. The customer sees the partner and tracking number.</p>}
    </div>
  );
}

export default function Orders() {
  const [page, setPage] = useState(1);
  const [status, setStatus] = useState('All');
  const itemsQuery = useProducerOrderItems({ status: status === 'All' ? undefined : status, page, pageSize: 15 });
  const revenueQuery = useProducerRevenue();
  const performanceQuery = useProducerProductPerformance();
  const { accept, reject, startProcessing, ship, deliver } = useProducerOrderMutations();
  const [shipForm, setShipForm] = useState({});
  const partners = useLogisticsDirectory().data || [];

  const items = itemsQuery.data?.items || [];

  return (
    <div>
      <PageHeader title="Orders & Fulfillment" description="Manage incoming orders and track your sales performance." />

      <QueryStatusBanner queries={[revenueQuery,performanceQuery]} />
      {[accept,reject,startProcessing,ship,deliver].map((mutation,index)=><MutationFeedback key={index} mutation={mutation} successMessage="Order updated." />)}
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
            onClick={() => { setStatus(f); setPage(1); }}
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
                    <Button variant="primary" disabled={accept.isPending} onClick={() => accept.mutate(item.id)}>Accept</Button>
                    <Button variant="secondary" disabled={reject.isPending} onClick={() => reject.mutate({ id: item.id, reason: 'Out of stock' })}>Reject</Button>
                  </>
                )}
                {item.producerStatus === 'Accepted' && (
                  <Button variant="primary" disabled={startProcessing.isPending} onClick={() => startProcessing.mutate(item.id)}>Start Processing</Button>
                )}
                {item.producerStatus === 'Processing' && (
                  <ShipControls
                    item={item}
                    partners={partners}
                    form={shipForm[item.id] || {}}
                    onChange={(patch) => setShipForm((prev) => ({ ...prev, [item.id]: { ...prev[item.id], ...patch } }))}
                    onShip={(payload) => ship.mutate({ id: item.id, payload })}
                    shipping={ship.isPending}
                  />
                )}
                {item.producerStatus === 'Shipped' && (
                  <Button variant="primary" disabled={deliver.isPending} onClick={() => deliver.mutate(item.id)}>Mark Delivered</Button>
                )}
              </div>
            </div>
          ))}
          {items.length === 0 && <p className="text-sm text-body/60">No orders in this status.</p>}
        </div>
        {itemsQuery.data?.totalPages > 1 && <Pagination currentPage={page} totalPages={itemsQuery.data.totalPages} onPageChange={setPage} />}
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
      <ProducerInsights />
    </div>
  );
}
