import { routePaths } from '../../routes/routePaths';
import { PageHeader } from '../../components/ui';
import { EntityCard } from '../../components/cards';
import { categories } from '../../data/mockData';

export default function Categories() {
  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Marketplace', path: routePaths.marketplace },
          { label: 'Categories' },
        ]}
        title="Categories"
        description="Browse heritage products organized by craft category."
      />
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {categories.map((category) => (
          <EntityCard
            key={category.id}
            title={category.name}
            subtitle={`${category.itemCount} items`}
            to={routePaths.marketplaceProducts}
          />
        ))}
      </div>
    </div>
  );
}
