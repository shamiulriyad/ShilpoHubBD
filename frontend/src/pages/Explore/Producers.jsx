import { routePaths } from '../../routes/routePaths';
import { PageHeader, FilterPanel, QueryState } from '../../components/ui';
import { EntityCard } from '../../components/cards';
import { useProducts } from '../../hooks/queries/useCatalog';

const uniqueSorted = (values) => [...new Set(values.filter(Boolean))].sort();

// NOTE: there is no dedicated producer-listing endpoint yet, so the directory is
// derived from the product catalog (distinct producers). Replace with a real
// `GET /api/producers` call once the backend exposes one.
export default function Producers() {
  const query = useProducts({ pageSize: 50, sortBy: 0 });
  const items = query.data?.items ?? [];

  const producers = Object.values(
    items.reduce((acc, p) => {
      if (!p.producerName || acc[p.producerName]) return acc;
      acc[p.producerName] = {
        name: p.producerName,
        district: p.districtName,
        craft: p.categoryName,
      };
      return acc;
    }, {}),
  );

  const filterGroups = [
    { label: 'Craft', options: uniqueSorted(producers.map((p) => p.craft)) },
    { label: 'District', options: uniqueSorted(producers.map((p) => p.district)) },
  ];

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Explore', path: routePaths.explore },
          { label: 'Producers' },
        ]}
        title="Producers"
        description="Artisans, farmers and makers behind ShilpoHub."
      />
      <div className="grid gap-6 lg:grid-cols-[260px_1fr]">
        <FilterPanel groups={filterGroups} />
        <QueryState
          query={query}
          emptyLabel="No producers to show yet."
          isEmpty={() => producers.length === 0}
        >
          {() => (
            <div className="grid grid-cols-2 gap-4 sm:grid-cols-3">
              {producers.map((producer) => (
                <EntityCard
                  key={producer.name}
                  title={producer.name}
                  subtitle={producer.craft}
                  meta={producer.district}
                />
              ))}
            </div>
          )}
        </QueryState>
      </div>
    </div>
  );
}
