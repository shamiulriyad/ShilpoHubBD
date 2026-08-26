import { routePaths } from '../../routes/routePaths';
import { PageHeader, AsyncState } from '../../components/ui';
import { EntityCard } from '../../components/cards';
import { useCategories } from '../../hooks/useCategories';

export default function Categories() {
  const { data, isLoading, isError, error } = useCategories();

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
        <AsyncState isLoading={isLoading} isError={isError} error={error}>
          {data?.map((category) => (
            <EntityCard
              key={category.id}
              title={category.name}
              subtitle={`${category.productCount} items`}
              to={`${routePaths.marketplaceProducts}?categoryId=${category.id}`}
            />
          ))}
        </AsyncState>
      </div>
    </div>
  );
}
