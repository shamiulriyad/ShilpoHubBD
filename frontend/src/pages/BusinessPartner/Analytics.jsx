import { PageHeader, SectionHeader, AsyncState } from '../../components/ui';
import { StatCard } from '../../components/cards';
import {
  useSpendingAnalytics,
  useProcurementAnalytics,
  useSupplierPerformance,
  useMarketDemand,
} from '../../hooks/useBusinessPartnerAnalytics';

export default function Analytics() {
  const spendingQuery = useSpendingAnalytics();
  const procurementQuery = useProcurementAnalytics();
  const supplierPerfQuery = useSupplierPerformance();
  const marketDemandQuery = useMarketDemand();

  const spending = spendingQuery.data;
  const procurement = procurementQuery.data;

  return (
    <div>
      <PageHeader title="Business Analytics" description="Spending, procurement and supplier performance insights." />

      <AsyncState isLoading={spendingQuery.isLoading} isError={spendingQuery.isError} error={spendingQuery.error}>
        {spending && (
          <div className="mb-10 grid grid-cols-2 gap-4 lg:grid-cols-4">
            <StatCard label="Total Spent" value={`৳ ${spending.totalSpent.toLocaleString()}`} />
            <StatCard label="Total Orders" value={spending.totalOrders} />
            <StatCard label="Avg. Order Value" value={`৳ ${Math.round(spending.averageOrderValue).toLocaleString()}`} />
            <StatCard label="Procurement Requests" value={procurement?.totalRequests ?? '—'} />
          </div>
        )}
      </AsyncState>

      <SectionHeader eyebrow="Spending" title="Spending by Category" />
      <div className="mb-10 divide-y divide-border rounded-xl border border-border bg-surface">
        {(spending?.spendingByCategory || []).map((c) => (
          <div key={c.categoryId} className="flex items-center justify-between p-3 text-sm">
            <span>{c.categoryName}</span>
            <span className="text-body/60">৳ {c.totalSpent.toLocaleString()} · {c.orderCount} orders</span>
          </div>
        ))}
        {(spending?.spendingByCategory || []).length === 0 && <p className="p-3 text-sm text-body/60">No spending data yet.</p>}
      </div>

      <SectionHeader eyebrow="Suppliers" title="Supplier Performance" />
      <AsyncState isLoading={supplierPerfQuery.isLoading} isError={supplierPerfQuery.isError} error={supplierPerfQuery.error}>
        <div className="mb-10 divide-y divide-border rounded-xl border border-border bg-surface">
          {(supplierPerfQuery.data || []).map((s) => (
            <div key={s.producerId} className="flex items-center justify-between p-3 text-sm">
              <span>{s.producerName}</span>
              <span className="text-body/60">★ {s.averageRating.toFixed(1)} · {s.completedProcurements}/{s.totalProcurements} completed</span>
            </div>
          ))}
          {(supplierPerfQuery.data || []).length === 0 && <p className="p-3 text-sm text-body/60">No supplier activity yet.</p>}
        </div>
      </AsyncState>

      <SectionHeader eyebrow="Market" title="Market Demand by Category" />
      <div className="divide-y divide-border rounded-xl border border-border bg-surface">
        {(marketDemandQuery.data || []).map((d) => (
          <div key={d.categoryId} className="flex items-center justify-between p-3 text-sm">
            <span>{d.categoryName}</span>
            <span className="text-body/60">{d.totalQuantityOrdered} units · ৳ {d.totalRevenue.toLocaleString()}</span>
          </div>
        ))}
        {(marketDemandQuery.data || []).length === 0 && <p className="p-3 text-sm text-body/60">No market data yet.</p>}
      </div>
    </div>
  );
}
