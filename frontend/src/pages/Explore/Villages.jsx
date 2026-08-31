import { routePaths } from '../../routes/routePaths';
import { PageHeader, FilterPanel, QueryState } from '../../components/ui';
import { VillageCard } from '../../components/cards';
import { useVillages } from '../../hooks/queries/useCatalog';

const uniqueSorted = (values) => [...new Set(values.filter(Boolean))].sort();

export default function Villages() {
  const query = useVillages();
  const villages = query.data ?? [];

  const filterGroups = [
    { label: 'District', options: uniqueSorted(villages.map((v) => v.districtName)) },
    { label: 'Craft', options: uniqueSorted(villages.map((v) => v.craft)) },
  ];

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Explore', path: routePaths.explore },
          { label: 'Heritage Villages' },
        ]}
        title="Heritage Villages"
        description="Villages recognized for keeping traditional crafts alive."
      />
      <div className="grid gap-6 lg:grid-cols-[260px_1fr]">
        <FilterPanel groups={filterGroups} />
        <QueryState query={query} emptyLabel="No heritage villages have been added yet.">
          {(items) => (
            <div className="grid grid-cols-2 gap-4 sm:grid-cols-3">
              {items.map((village) => (
                <VillageCard
                  key={village.id}
                  village={{ ...village, district: village.districtName }}
                  to={routePaths.exploreVillageDetails.replace(':villageId', village.id)}
                />
              ))}
            </div>
          )}
        </QueryState>
      </div>
    </div>
  );
}
