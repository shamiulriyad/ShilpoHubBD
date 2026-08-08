import { routePaths } from '../../routes/routePaths';
import { PageHeader, SectionHeader, AnalyticsChart, Table } from '../../components/ui';
import { StatCard } from '../../components/cards';
import { orders, categories } from '../../data/mockData';

export default function PurchaseAnalytics() {
  const totalSpent = orders.reduce((sum, order) => sum + order.total, 0);
  const avgOrder = Math.round(totalSpent / (orders.length || 1));
  const topCategory = categories[0];

  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Purchase Analytics' }]}
        title="Purchase Analytics"
        description="A look at your spending and shopping habits on ShilpoHub."
      />

      <div className="mb-10 grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatCard label="Total Spent" value={`৳ ${totalSpent.toLocaleString()}`} />
        <StatCard label="Orders Placed" value={orders.length} />
        <StatCard label="Avg. Order Value" value={`৳ ${avgOrder.toLocaleString()}`} />
        <StatCard label="Favorite Category" value={categories[0]?.name || '—'} />
      </div>

      <div className="mb-10 grid gap-6 lg:grid-cols-2">
        <AnalyticsChart title="Monthly Spending" type="line" value={`৳ ${totalSpent.toLocaleString()}`} trend="+12% vs last quarter" />
        <AnalyticsChart title="Spending by Category" type="donut" value={topCategory?.name} />
      </div>

      <SectionHeader eyebrow="Breakdown" title="Spending by Category" />
      <Table
        columns={['Category', 'Items']}
        rows={categories.map((c) => ({ Category: c.name, Items: c.itemCount }))}
      />
    </div>
  );
}
