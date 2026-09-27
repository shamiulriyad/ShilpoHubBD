import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState, AnalyticsChart } from '../../components/ui';
import MutationFeedback from '../../components/ui/MutationFeedback';
import { useProducts } from '../../hooks/useProducts';
import { useProductIntelligence, useProductIntelligenceAiInsights } from '../../hooks/useProductIntelligence';

const RANGE_OPTIONS = [
  { value: 'Last30Days', label: 'Last 30 days' },
  { value: 'Last3Months', label: 'Last 3 months' },
  { value: 'Last5Months', label: 'Last 5 months' },
  { value: 'Last12Months', label: 'Last 12 months' },
];

const money = (value) => `৳${Number(value ?? 0).toLocaleString('en-BD')}`;

export default function ProductIntelligence() {
  const [search, setSearch] = useState('');
  const [selectedProduct, setSelectedProduct] = useState(null);
  const [range, setRange] = useState('Last30Days');

  const searchQuery = useProducts({ search: search || undefined, pageSize: 10 });
  const results = searchQuery.data?.items || [];

  const dataQuery = useProductIntelligence(selectedProduct?.id, range);
  const aiInsights = useProductIntelligenceAiInsights();

  const data = dataQuery.data;

  return (
    <div>
      <PageHeader title="Product Intelligence" description="Select a product to analyze its real business performance and get AI-assisted suggestions." />

      <div className="mb-6 space-y-3">
        <input
          placeholder="Search products by name…" value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
        />
        {search && (
          <div className="space-y-2">
            {results.map((p) => (
              <button
                key={p.id} type="button"
                onClick={() => { setSelectedProduct(p); aiInsights.reset(); }}
                className={`block w-full rounded-lg border p-3 text-left text-sm transition ${selectedProduct?.id === p.id ? 'border-primary bg-primary/5' : 'border-border bg-surface hover:shadow-md'}`}
              >
                {p.name} <span className="text-body/60">· {p.producerName}</span>
              </button>
            ))}
            {results.length === 0 && !searchQuery.isLoading && <p className="text-sm text-body/60">No products match.</p>}
          </div>
        )}
      </div>

      {selectedProduct && (
        <div className="space-y-6">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <p className="text-lg font-semibold text-heading">{selectedProduct.name}</p>
            <select value={range} onChange={(e) => { setRange(e.target.value); aiInsights.reset(); }} className="rounded-md border border-border bg-background px-3 py-2 text-sm">
              {RANGE_OPTIONS.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
            </select>
          </div>

          <AsyncState isLoading={dataQuery.isLoading} isError={dataQuery.isError} error={dataQuery.error}>
            {data && (
              <>
                {!data.hasSufficientHistory && (
                  <div className="rounded-xl border border-border bg-surface p-4 text-sm text-body/70">
                    Not enough delivered-order history in this range to show a meaningful trend yet. Metrics below reflect what data does exist.
                  </div>
                )}

                <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
                  <SummaryTile label="Average rating" value={`★ ${data.averageRating.toFixed(1)} (${data.totalReviewCount})`} />
                  <SummaryTile label="Total views" value={data.totalViews.toLocaleString()} />
                  <SummaryTile label="Wishlist (current)" value={data.currentWishlistCount.toLocaleString()} />
                  <SummaryTile
                    label="Revenue growth"
                    value={data.summary.revenueGrowthPercent == null ? '—' : `${data.summary.revenueGrowthPercent >= 0 ? '+' : ''}${data.summary.revenueGrowthPercent}%`}
                  />
                </div>

                <div className="rounded-xl border border-border bg-surface p-4">
                  <p className="text-sm font-semibold text-heading">Inventory</p>
                  <p className="text-sm text-body/70">
                    Current stock: {data.inventory.currentStock}
                    {data.inventory.lowStockThreshold != null && ` (threshold: ${data.inventory.lowStockThreshold})`}
                    {data.inventory.isLowStock && <Badge tone="secondary" className="ml-2">Low stock</Badge>}
                  </p>
                </div>

                {!data.searchInterestAvailable && (
                  <p className="text-xs text-body/50">Search interest: {data.searchInterestUnavailableReason}</p>
                )}

                <div className="grid gap-4 lg:grid-cols-2">
                  <AnalyticsChart title="Revenue trend" type="line" data={data.periods} labelKey="periodLabel" valueKey="revenue" valueFormatter={money} />
                  <AnalyticsChart title="Units sold trend" type="line" data={data.periods} labelKey="periodLabel" valueKey="unitsSold" />
                  <AnalyticsChart title="Order trend" type="line" data={data.periods} labelKey="periodLabel" valueKey="orderCount" />
                  <AnalyticsChart title="Review trend" type="bar" data={data.periods} labelKey="periodLabel" valueKey="newReviews" />
                  <AnalyticsChart title="Wishlist adds trend" type="bar" data={data.periods} labelKey="periodLabel" valueKey="wishlistAdds" />
                  <AnalyticsChart title="Inventory movement" type="bar" data={data.inventory.movements} labelKey="periodLabel" valueKey="netChange" />
                </div>

                <div className="rounded-xl border border-border bg-surface p-5">
                  <div className="mb-3 flex items-center justify-between">
                    <p className="text-sm font-semibold text-heading">AI-assisted business suggestions</p>
                    <Button variant="primary" disabled={aiInsights.isPending} onClick={() => aiInsights.mutate({ productId: selectedProduct.id, range })}>
                      {aiInsights.isPending ? 'Analyzing…' : 'Generate insights'}
                    </Button>
                  </div>
                  <MutationFeedback mutation={aiInsights} />
                  {aiInsights.data && <AiInsightsPanel insights={aiInsights.data} />}
                </div>
              </>
            )}
          </AsyncState>
        </div>
      )}
    </div>
  );
}

function SummaryTile({ label, value }) {
  return (
    <div className="rounded-xl border border-border bg-surface p-4">
      <p className="text-xs text-body/50">{label}</p>
      <p className="text-lg font-semibold text-heading">{value}</p>
    </div>
  );
}

function AiInsightsPanel({ insights }) {
  return (
    <div className="space-y-3 border-t border-border pt-4">
      <p className="text-xs italic text-body/50">{insights.disclaimer}</p>
      <Row label="Demand trend" value={insights.demandTrend} />
      <Row label="Estimated next-period demand" value={insights.estimatedNextPeriodDemand} />
      <Row label="Sales trend interpretation" value={insights.salesTrendInterpretation} />
      <Row label="Inventory recommendation" value={insights.inventoryRecommendation} />
      <Row label="Pricing observation" value={insights.pricingObservation} />
      {insights.marketingOpportunities?.length > 0 && (
        <div>
          <p className="text-xs font-semibold uppercase tracking-wide text-body/50">Marketing opportunities</p>
          <ul className="list-inside list-disc text-sm text-body/70">{insights.marketingOpportunities.map((m, i) => <li key={i}>{m}</li>)}</ul>
        </div>
      )}
      {insights.riskIndicators?.length > 0 && (
        <div>
          <p className="text-xs font-semibold uppercase tracking-wide text-body/50">Risk indicators</p>
          <ul className="list-inside list-disc text-sm text-body/70">{insights.riskIndicators.map((r, i) => <li key={i}>{r}</li>)}</ul>
        </div>
      )}
      <Badge tone={insights.isAiGenerated ? 'success' : 'secondary'}>{insights.isAiGenerated ? 'AI-generated' : 'Rule-based (AI unavailable)'}</Badge>
    </div>
  );
}

function Row({ label, value }) {
  if (!value) return null;
  return (
    <div>
      <p className="text-xs font-semibold uppercase tracking-wide text-body/50">{label}</p>
      <p className="text-sm text-body/70">{value}</p>
    </div>
  );
}
