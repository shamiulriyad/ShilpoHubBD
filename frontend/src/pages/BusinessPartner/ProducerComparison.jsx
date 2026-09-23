import { useState } from 'react';
import { PageHeader, Button } from '../../components/ui';
import { useProducerComparison } from '../../hooks/useProducerComparison';
import { ProducerMultiSelect } from '../../components/forms/EntityPickers';

export default function ProducerComparison() {
  const [producerIds, setProducerIds] = useState([]);
  const compare = useProducerComparison();

  const handleSubmit = (event) => {
    event.preventDefault();
    if (producerIds.length >= 2) compare.mutate(producerIds);
  };

  const rows = compare.data || [];

  return (
    <div>
      <PageHeader title="Compare Producers" description="Compare producers side by side on price, quality, delivery and certifications." />

      <form onSubmit={handleSubmit} className="mb-8 space-y-4">
        <ProducerMultiSelect value={producerIds} onChange={setProducerIds} />
        <div className="flex items-center gap-3">
          <Button type="submit" variant="primary" disabled={compare.isPending || producerIds.length < 2}>
            {compare.isPending ? 'Comparing…' : 'Compare'}
          </Button>
          {producerIds.length < 2 && <span className="text-xs text-body/60">Select at least 2 producers.</span>}
        </div>
      </form>

      {rows.length > 0 && (
        <div className="overflow-x-auto rounded-xl border border-border bg-surface">
          <table className="w-full min-w-[720px] text-left text-sm">
            <thead>
              <tr className="border-b border-border bg-background/60 text-xs uppercase tracking-wide text-body/60">
                <th className="px-4 py-3">Producer</th>
                <th className="px-4 py-3">District</th>
                <th className="px-4 py-3">Rating</th>
                <th className="px-4 py-3">Price Range</th>
                <th className="px-4 py-3">Products</th>
                <th className="px-4 py-3">Handmade %</th>
                <th className="px-4 py-3">Avg. Delivery</th>
                <th className="px-4 py-3">Orders Fulfilled</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((row) => (
                <tr key={row.producerId} className="border-b border-border last:border-0">
                  <td className="px-4 py-3 font-medium text-heading">{row.producerName}</td>
                  <td className="px-4 py-3">{row.districtName || '—'}</td>
                  <td className="px-4 py-3">★ {row.averageRating.toFixed(1)} ({row.totalReviewCount})</td>
                  <td className="px-4 py-3">৳{row.minPrice?.toLocaleString() ?? '—'}–{row.maxPrice?.toLocaleString() ?? '—'}</td>
                  <td className="px-4 py-3">{row.productCount}</td>
                  <td className="px-4 py-3">{Math.round(row.handmadeVerifiedRatio * 100)}%</td>
                  <td className="px-4 py-3">{row.averageDeliveryDays ? `${row.averageDeliveryDays.toFixed(1)}d` : '—'}</td>
                  <td className="px-4 py-3">{row.totalOrdersFulfilled}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
