import { PageHeader } from '../../components/ui';
import { EntityCard } from '../../components/cards';
import { producers } from '../../data/mockData';
import { routePaths } from '../../routes/routePaths';

export default function DashboardCommunity() {
  return (
    <div>
      <PageHeader title="Community" description="Connect with producers, partners and fellow members." />
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 xl:grid-cols-4">
        {producers.map((producer) => (
          <EntityCard
            key={producer.id}
            title={producer.name}
            subtitle={producer.craft}
            meta={producer.district}
            to={routePaths.exploreProducers}
          />
        ))}
      </div>
    </div>
  );
}
