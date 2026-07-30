import { routePaths } from '../../routes/routePaths';
import { PageHeader, FilterPanel } from '../../components/ui';
import { EntityCard } from '../../components/cards';
import { producers, districts, crafts } from '../../data/mockData';

const filterGroups = [
  { label: 'Craft', options: crafts.map((c) => c.name) },
  { label: 'District', options: districts.slice(0, 5).map((d) => d.name) },
];

export default function Producers() {
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
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3">
          {producers.map((producer) => (
            <EntityCard
              key={producer.id}
              title={producer.name}
              subtitle={producer.craft}
              meta={`${producer.district} · ★ ${producer.rating}`}
              to={routePaths.exploreProducerDetails.replace(':producerId', producer.id)}
            />
          ))}
        </div>
      </div>
    </div>
  );
}
