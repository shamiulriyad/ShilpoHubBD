import { routePaths } from '../../routes/routePaths';
import { PageHeader, FilterPanel, AsyncState } from '../../components/ui';
import { VillageCard } from '../../components/cards';
import { useVillages } from '../../hooks/useVillages';
import { useDistricts } from '../../hooks/useDistricts';
import { toVillageCardItem } from '../../utils/villageAdapters';

export default function Villages() {
  const villagesQuery = useVillages();
  const districtsQuery = useDistricts();

  const filterGroups = [{ label: 'District', options: (districtsQuery.data || []).slice(0, 8).map((d) => d.name) }];

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
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3">
          <AsyncState isLoading={villagesQuery.isLoading} isError={villagesQuery.isError} error={villagesQuery.error}>
            {villagesQuery.data?.map((village) => (
              <VillageCard
                key={village.id}
                village={toVillageCardItem(village)}
                to={routePaths.exploreVillageDetails.replace(':villageId', village.id)}
              />
            ))}
          </AsyncState>
        </div>
      </div>
    </div>
  );
}
