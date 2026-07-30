import { routePaths } from '../../routes/routePaths';
import { PageHeader, FilterPanel } from '../../components/ui';
import { FestivalCard } from '../../components/cards';
import { festivals, districts } from '../../data/mockData';

const filterGroups = [{ label: 'District', options: districts.slice(0, 5).map((d) => d.name) }];

export default function FestivalDirectory() {
  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Tourism', path: routePaths.tourism },
          { label: 'Festivals' },
        ]}
        title="Festival Directory"
        description="Seasonal and regional cultural festivals."
      />
      <div className="grid gap-6 lg:grid-cols-[260px_1fr]">
        <FilterPanel groups={filterGroups} />
        <div className="grid gap-4 sm:grid-cols-2">
          {festivals.map((festival) => (
            <FestivalCard key={festival.id} festival={festival} />
          ))}
        </div>
      </div>
    </div>
  );
}
