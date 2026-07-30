import { routePaths } from '../../routes/routePaths';
import { PageHeader, FilterPanel } from '../../components/ui';
import { VillageCard } from '../../components/cards';
import { villages, districts } from '../../data/mockData';

const filterGroups = [
  { label: 'District', options: districts.slice(0, 5).map((d) => d.name) },
  { label: 'Craft', options: ['Weaving', 'Pottery', 'Bamboo Work', 'Terracotta'] },
];

export default function Villages() {
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
          {villages.map((village) => (
            <VillageCard key={village.id} village={village} to={routePaths.exploreVillageDetails.replace(':villageId', village.id)} />
          ))}
        </div>
      </div>
    </div>
  );
}
