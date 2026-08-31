import { PageHeader, AsyncState } from '../../components/ui';
import { EntityCard } from '../../components/cards';
import { routePaths } from '../../routes/routePaths';
import { useDistricts } from '../../hooks/useDistricts';
import { useVillages } from '../../hooks/useVillages';
import { useCategories } from '../../hooks/useCategories';

export default function DashboardExplore() {
  const districtsQuery = useDistricts();
  const villagesQuery = useVillages();
  const categoriesQuery = useCategories();

  const districts = districtsQuery.data || [];
  const villages = villagesQuery.data || [];
  const crafts = categoriesQuery.data || [];

  const isLoading = districtsQuery.isLoading || villagesQuery.isLoading || categoriesQuery.isLoading;
  const isError = districtsQuery.isError || villagesQuery.isError || categoriesQuery.isError;

  return (
    <div>
      <PageHeader title="Explore" description="Saved districts, villages and crafts you follow." />
      <AsyncState isLoading={isLoading} isError={isError} error={districtsQuery.error}>
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 xl:grid-cols-4">
          {districts.slice(0, 4).map((d) => (
            <EntityCard key={d.id} title={d.name} subtitle={d.division} to={routePaths.exploreDistricts} />
          ))}
          {villages.slice(0, 4).map((v) => (
            <EntityCard key={v.id} title={v.name} subtitle={v.craft} to={routePaths.exploreVillages} />
          ))}
          {crafts.slice(0, 4).map((c) => (
            <EntityCard key={c.id} title={c.name} subtitle={c.description} to={routePaths.exploreCrafts} />
          ))}
        </div>
      </AsyncState>
    </div>
  );
}
