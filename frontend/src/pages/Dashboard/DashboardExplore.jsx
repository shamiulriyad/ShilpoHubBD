import { PageHeader } from '../../components/ui';
import { EntityCard } from '../../components/cards';
import { districts, villages, crafts } from '../../data/mockData';
import { routePaths } from '../../routes/routePaths';

export default function DashboardExplore() {
  return (
    <div>
      <PageHeader title="Explore" description="Saved districts, villages and crafts you follow." />
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 xl:grid-cols-4">
        {districts.slice(0, 4).map((d) => (
          <EntityCard key={d.id} title={d.name} subtitle={`${d.villages} villages`} to={routePaths.exploreDistricts} />
        ))}
        {villages.slice(0, 4).map((v) => (
          <EntityCard key={v.id} title={v.name} subtitle={v.craft} to={routePaths.exploreVillages} />
        ))}
        {crafts.slice(0, 4).map((c) => (
          <EntityCard key={c.id} title={c.name} subtitle={c.category} to={routePaths.exploreCrafts} />
        ))}
      </div>
    </div>
  );
}
