import { routePaths } from '../../routes/routePaths';
import { PageHeader } from '../../components/ui';
import { EntityCard } from '../../components/cards';
import { districts } from '../../data/mockData';

export default function Districts() {
  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Explore', path: routePaths.explore },
          { label: 'Districts' },
        ]}
        title="Districts"
        description="Browse heritage villages, crafts and producers by district."
      />
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {districts.map((district) => (
          <EntityCard
            key={district.id}
            title={district.name}
            subtitle={`${district.villages} villages · ${district.crafts} crafts`}
            to={routePaths.exploreDistrictDetails.replace(':districtId', district.id)}
          />
        ))}
      </div>
    </div>
  );
}
