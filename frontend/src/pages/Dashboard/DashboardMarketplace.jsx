import { PageHeader, Table, AsyncState } from '../../components/ui';
import { DashboardCard, StatCard } from '../../components/cards';
import { useMyProducts } from '../../hooks/useProducts';

export default function DashboardMarketplace() {
  const { data, isLoading, isError, error } = useMyProducts();
  const listings = data || [];

  return (
    <div>
      <PageHeader title="Marketplace" description="Your listings, orders and marketplace performance." />
      <div className="mb-6 grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatCard label="Active Listings" value={listings.filter((p) => p.isActive).length} />
        <StatCard label="Total Listings" value={listings.length} />
        <StatCard label="Featured" value={listings.filter((p) => p.isFeatured).length} />
        <StatCard label="Total Stock" value={listings.reduce((sum, p) => sum + (p.stock || 0), 0)} />
      </div>
      <DashboardCard title="Your Listings">
        <AsyncState isLoading={isLoading} isError={isError} error={error}>
          <Table
            columns={['name', 'category', 'price']}
            rows={listings.map((p) => ({
              name: p.name,
              category: p.categoryName,
              price: `৳ ${(p.price ?? 0).toLocaleString()}`,
            }))}
          />
          {listings.length === 0 && (
            <p className="p-4 text-sm text-body/60">You have no listings yet.</p>
          )}
        </AsyncState>
      </DashboardCard>
    </div>
  );
}
