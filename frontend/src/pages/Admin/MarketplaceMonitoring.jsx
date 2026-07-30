import { PageHeader, Table, ChartPlaceholder, Badge } from '../../components/ui';
import { StatCard } from '../../components/cards';
import { products, auctions } from '../../data/mockData';

export default function MarketplaceMonitoring() {
  return (
    <div>
      <PageHeader title="Marketplace Monitoring" description="Oversee listings, orders and auctions." />
      <div className="mb-6 grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatCard label="Total Listings" value={products.length * 40} />
        <StatCard label="Active Auctions" value={auctions.length} />
        <StatCard label="Orders Today" value="86" />
        <StatCard label="Disputes Open" value="2" />
      </div>
      <div className="mb-6">
        <ChartPlaceholder title="Marketplace Volume by Category" type="bar" />
      </div>
      <p className="mb-3 text-sm font-semibold text-heading">Recent Listings</p>
      <Table
        columns={['name', 'category', 'price', 'status']}
        rows={products.map((p) => ({
          name: p.name,
          category: p.category,
          price: `৳ ${p.price}`,
          status: <Badge tone="success">Approved</Badge>,
        }))}
      />
    </div>
  );
}
