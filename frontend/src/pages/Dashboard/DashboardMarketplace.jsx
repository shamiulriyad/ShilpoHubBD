import { PageHeader, Table } from '../../components/ui';
import { DashboardCard, StatCard } from '../../components/cards';
import { products } from '../../data/mockData';

export default function DashboardMarketplace() {
  return (
    <div>
      <PageHeader title="Marketplace" description="Your listings, orders and marketplace performance." />
      <div className="mb-6 grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatCard label="Active Listings" value="18" />
        <StatCard label="Pending Orders" value="6" />
        <StatCard label="Total Sales" value="৳ 1,24,500" />
        <StatCard label="Wishlist Adds" value="212" />
      </div>
      <DashboardCard title="Your Listings">
        <Table
          columns={['name', 'category', 'price']}
          rows={products.slice(0, 5).map((p) => ({ name: p.name, category: p.category, price: `৳ ${p.price}` }))}
        />
      </DashboardCard>
    </div>
  );
}
