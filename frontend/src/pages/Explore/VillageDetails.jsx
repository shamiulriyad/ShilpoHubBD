import { useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader } from '../../components/ui';
import { EntityCard } from '../../components/cards';
import { villages, producers } from '../../data/mockData';

export default function VillageDetails() {
  const { villageId } = useParams();
  const village = villages.find((v) => v.id === villageId) || villages[0];

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Explore', path: routePaths.explore },
          { label: 'Heritage Villages', path: routePaths.exploreVillages },
          { label: village.name },
        ]}
        title={village.name}
        description={`${village.craft} · ${village.district}`}
      />

      <div className="mb-10 flex aspect-[21/9] items-center justify-center rounded-2xl border border-border bg-background text-sm text-body/40">
        Village Gallery Placeholder
      </div>

      <p className="mb-3 text-sm font-semibold text-heading">Producers in {village.name}</p>
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {producers.map((producer) => (
          <EntityCard
            key={producer.id}
            title={producer.name}
            subtitle={producer.craft}
            to={routePaths.exploreProducerDetails.replace(':producerId', producer.id)}
          />
        ))}
      </div>
    </div>
  );
}
