import { PageHeader, AsyncState } from '../../components/ui';
import { EntityCard } from '../../components/cards';
import { routePaths } from '../../routes/routePaths';
import { useProducts } from '../../hooks/useProducts';

export default function DashboardCommunity() {
  const { data, isLoading, isError, error } = useProducts({ pageSize: 24 });

  // No producer-directory endpoint — derive distinct producers from the catalog.
  const producers = [
    ...new Map(
      (data?.items || [])
        .filter((p) => p.producerName)
        .map((p) => [p.producerName, { name: p.producerName, craft: p.categoryName, district: p.districtName }]),
    ).values(),
  ];

  return (
    <div>
      <PageHeader title="Community" description="Connect with producers, partners and fellow members." />
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 xl:grid-cols-4">
          {producers.map((producer) => (
            <EntityCard
              key={producer.name}
              title={producer.name}
              subtitle={producer.craft}
              meta={producer.district}
              to={routePaths.exploreProducers}
            />
          ))}
          {producers.length === 0 && (
            <p className="col-span-full text-sm text-body/60">No producers to show yet.</p>
          )}
        </div>
      </AsyncState>
    </div>
  );
}
