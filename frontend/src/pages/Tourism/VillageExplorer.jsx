import { routePaths } from '../../routes/routePaths';
import { PageHeader } from '../../components/ui';
import { VillageCard } from '../../components/cards';
import { villages } from '../../data/mockData';

export default function VillageExplorer() {
  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Tourism', path: routePaths.tourism },
          { label: 'Village Explorer' },
        ]}
        title="Village Explorer"
        description="Plan a visit to Bangladesh's heritage craft villages."
      />
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {villages.map((village) => (
          <VillageCard key={village.id} village={village} to={routePaths.exploreVillageDetails.replace(':villageId', village.id)} />
        ))}
      </div>
    </div>
  );
}
