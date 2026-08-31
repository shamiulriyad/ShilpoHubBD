import { routePaths } from '../../routes/routePaths';
import { PageHeader, SectionHeader, AnalyticsChart, Table, AsyncState } from '../../components/ui';
import { StatCard } from '../../components/cards';
import { usePurchaseAnalytics, useFavoriteCategories } from '../../hooks/useAnalytics';

export default function PurchaseAnalytics() {
  const purchasesQuery = usePurchaseAnalytics();
  const favoriteCategoriesQuery = useFavoriteCategories(10);

  const purchases = purchasesQuery.data;
  const favoriteCategories = favoriteCategoriesQuery.data || [];

  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Purchase Analytics' }]}
        title="Purchase Analytics"
        description="A look at your spending and shopping habits on ShilpoHub."
      />

      <AsyncState isLoading={purchasesQuery.isLoading} isError={purchasesQuery.isError} error={purchasesQuery.error}>
        {purchases && (
          <div className="mb-10 grid grid-cols-2 gap-4 lg:grid-cols-4">
            <StatCard label="Total Spent" value={`৳ ${purchases.totalSpent.toLocaleString()}`} />
            <StatCard label="Orders Placed" value={purchases.totalOrders} />
            <StatCard label="Avg. Order Value" value={`৳ ${Math.round(purchases.averageOrderValue).toLocaleString()}`} />
            <StatCard label="Favorite Category" value={favoriteCategories[0]?.categoryName || '—'} />
          </div>
        )}
      </AsyncState>

      <div className="mb-10 grid gap-6 lg:grid-cols-2">
        <AnalyticsChart
          title="Monthly Spending"
          type="line"
          value={purchases ? `৳ ${purchases.totalSpent.toLocaleString()}` : undefined}
        />
        <AnalyticsChart title="Spending by Category" type="donut" value={favoriteCategories[0]?.categoryName} />
      </div>

      <SectionHeader eyebrow="Breakdown" title="Spending by Category" />
      <AsyncState isLoading={favoriteCategoriesQuery.isLoading} isError={favoriteCategoriesQuery.isError} error={favoriteCategoriesQuery.error}>
        <Table
          columns={['Category', 'Items', 'Total Spent']}
          rows={favoriteCategories.map((c) => ({
            Category: c.categoryName,
            Items: c.itemCount,
            'Total Spent': `৳ ${c.totalSpent.toLocaleString()}`,
          }))}
        />
        {favoriteCategories.length === 0 && <p className="mt-4 text-sm text-body/60">No purchases yet.</p>}
      </AsyncState>
    </div>
  );
}
